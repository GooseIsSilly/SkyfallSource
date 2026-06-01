namespace TPSBR
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Fusion;
    using Fusion.Plugin;
    using Fusion.Sockets;
    using Fusion.Addons.Physics;

    using LogType = UnityEngine.LogType;
    using Random = UnityEngine.Random;
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;

    public sealed class NetworkGame : ContextBehaviour, IPlayerJoined, IPlayerLeft
    {
        // PUBLIC MEMBERS

        public LevelGenerator LevelGenerator => _levelGenerator;

        public List<Player> ActivePlayers = new List<Player>();
        public int ActivePlayerCount = 0;

        /// <summary>
        /// True from PrepareForMigrationResume() until Activate() completes on the new host.
        /// GameplayMode subclasses must skip CheckWinCondition while this is set to prevent a
        /// false 1-player win trigger caused by the departing host's player being removed before
        /// migration is complete.
        /// </summary>
        public bool IsMigrating { get; private set; }

        // PRIVATE MEMBERS

        [SerializeField]
        private Player _playerPrefab;
        [SerializeField]
        private GameplayMode[] _modePrefabs;

        [Header("Level Generation")]
        [SerializeField]
        private LevelGenerator _levelGenerator;
        [SerializeField]
        private int _fixedSeed = 0;
        [SerializeField]
        private int _levelSize = 30;
        [SerializeField]
        private int _areaCount = 5;
        [SerializeField]
        private int _maxItemBoxes = 0;

        [Space]
        [SerializeField]
        private ShrinkingArea _shrinkingArea;

        [Networked]
        private int _levelGeneratorSeed { get; set; }

        private PlayerRef _localPlayer;
        private Dictionary<PlayerRef, Player> _pendingPlayers = new Dictionary<PlayerRef, Player>();
        private Dictionary<string, Player> _disconnectedPlayers = new Dictionary<string, Player>();
        private FusionCallbacksHandler _fusionCallbacks = new FusionCallbacksHandler();
        private List<Player> _spawnedPlayers = new List<Player>(byte.MaxValue);
        private List<Player> _allPlayers = new List<Player>(byte.MaxValue);
        private StatsRecorder _statsRecorder;
        private LogRecorder _logRecorder;
        private GameplayMode _gameplayMode;
        private SessionRequest _sessionRequest;
        private bool _levelGenerated;
        private bool _isActive;

        // PUBLIC METHODS

        /// <summary>Initializes the network game for a fresh start or a host migration resume.</summary>
        public void Initialize(SessionRequest request)
        {
            bool isMigration = request.HostMigrationToken != null;

            if (HasStateAuthority == true)
            {
                if (isMigration)
                {
                    // GameplayMode was re-spawned from the snapshot in HostMigrationResume, but
                    // its Spawned() callback (which assigns Context.GameplayMode) fires on the
                    // first simulation tick — not synchronously. Cache what we have; if it is
                    // still null, Activate() will resolve it once the coroutine confirms
                    // Context.GameplayMode is non-null.
                    _gameplayMode = Context.GameplayMode;
                    if (_gameplayMode == null)
                    {
                        Debug.Log("[NetworkGame] GameplayMode not yet in context after host migration; will resolve in Activate().");
                    }
                }
                else
                {
                    var prefab = _modePrefabs.Find(t => t != null && t.Type == request.GameplayType);

                    if (prefab == null)
                    {
                        Debug.LogError($"[NetworkGame] No gameplay mode prefab found for type: {request.GameplayType}. Please check that mode prefabs are assigned in the NetworkGame component.");
                        return;
                    }

                    _gameplayMode = Runner.Spawn(prefab);
                }
            }

            _localPlayer = Runner.LocalPlayer;
            _sessionRequest = request;

            _fusionCallbacks.HostMigration -= OnHostMigration;
            _fusionCallbacks.HostMigration += OnHostMigration;
            _fusionCallbacks.DisconnectedFromServer -= OnDisconnectedFromServer;
            _fusionCallbacks.DisconnectedFromServer += OnDisconnectedFromServer;

            Runner.RemoveCallbacks(_fusionCallbacks);
            Runner.AddCallbacks(_fusionCallbacks);

            ActivePlayers.Clear();
            ActivePlayerCount = 0;
        }

        public void Activate()
        {
            _isActive = true;

            // Migration is fully complete once Activate() runs — the new host is live,
            // all snapshot objects are restored, and win-condition checks are safe again.
            IsMigrating = false;

            bool isMigration = _sessionRequest.HostMigrationToken != null;

            if (HasStateAuthority == false)
            {
                GenerateLevel(_levelGeneratorSeed);

                if (ApplicationSettings.IsStrippedBatch == true)
                {
                    Runner.GetComponent<RunnerSimulatePhysics3D>().enabled = false;
                    Runner.LagCompensation.enabled = false;
                }

                return;
            }

            // The coroutine waits until Context.GameplayMode is non-null before calling
            // Activate(), so it is safe to resolve _gameplayMode here if it was still
            // null when Initialize() ran (Spawned() had not yet fired at that point).
            if (isMigration && _gameplayMode == null)
            {
                _gameplayMode = Context.GameplayMode;
                if (_gameplayMode == null)
                {
                    Debug.LogError("[NetworkGame] GameplayMode still not in context during Activate() after host migration.");
                    return;
                }
            }

            if (_levelGenerator != null && _levelGenerator.enabled == true)
            {
                if (isMigration)
                {
                    // Level objects are already in the snapshot; prevent a re-spawn by marking as generated.
                    _levelGenerated = true;
                }
                else
                {
                    _levelGeneratorSeed = _fixedSeed == 0 ? Random.Range(999, 999999999) : _fixedSeed;
                    GenerateLevel(_levelGeneratorSeed);
                }
            }

            if (_gameplayMode != null)
            {
                _gameplayMode.Activate();
            }

            foreach (var playerRef in Runner.ActivePlayers)
            {
                SpawnPlayer(playerRef);
            }
        }

        public Player GetPlayer(PlayerRef playerRef)
        {
            if (playerRef.IsRealPlayer == false)
                return default;
            if (Object == null)
                return default;

            _spawnedPlayers.Clear();
            Runner.GetAllBehaviours<Player>(_spawnedPlayers);

            for (int i = 0, count = _spawnedPlayers.Count; i < count; ++i)
            {
                Player player = _spawnedPlayers[i];
                if (player.Object.InputAuthority == playerRef)
                    return player;
            }

            return default;
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            if (HasStateAuthority == false)
            {
                if (_levelGenerated == false && _levelGeneratorSeed != default)
                {
                    GenerateLevel(_levelGeneratorSeed);
                }
            }

            Runner.SetIsSimulated(Object, true);
        }

        public override void FixedUpdateNetwork()
        {
            bool hasStateAuthority = HasStateAuthority;

            _allPlayers.Clear();
            Runner.GetAllBehaviours<Player>(_allPlayers);

            for (int i = _allPlayers.Count - 1; i >= 0; --i)
            {
                Player player = _allPlayers[i];

                PlayerRef inputAuthority = player.Object.InputAuthority;
                if (inputAuthority.IsRealPlayer == true)
                {
                    if (hasStateAuthority == true && Runner.IsPlayerValid(inputAuthority) == false)
                    {
                        _allPlayers.RemoveAt(i);
                        OnPlayerLeft(player);
                    }
                }
                else
                {
                    _allPlayers.RemoveAt(i);
                }
            }

            ActivePlayers.Clear();
            ActivePlayerCount = 0;

            foreach (Player player in _allPlayers)
            {
                if (player.UserID.HasValue() == false)
                    continue;

                ActivePlayers.Add(player);

                var statistics = player.Statistics;
                if (statistics.IsValid == false)
                    continue;

                if (statistics.IsEliminated == false)
                {
                    ++ActivePlayerCount;
                }
            }

            if (HasStateAuthority == false)
            {
                if (_levelGenerated == false && _levelGeneratorSeed != default && Runner.IsForward == true)
                {
                    GenerateLevel(_levelGeneratorSeed);
                }

                return;
            }

            if (_pendingPlayers.Count == 0)
                return;

            var playersToRemove = ListPool.Get<PlayerRef>(128);

            foreach (var playerPair in _pendingPlayers)
            {
                var playerRef = playerPair.Key;
                var player = playerPair.Value;

                if (player.IsInitialized == false)
                    continue;

                playersToRemove.Add(playerRef);

                if (_disconnectedPlayers.TryGetValue(player.UserID, out Player disconnectedPlayer) == true)
                {
                    _disconnectedPlayers.Remove(player.UserID);

                    int activePlayerIndex = ActivePlayers.IndexOf(player);
                    if (activePlayerIndex >= 0)
                    {
                        ActivePlayers[activePlayerIndex] = disconnectedPlayer;
                    }

                    disconnectedPlayer.OnReconnect(player);

                    // Remove original player, this is returning disconnected player
                    player.Object.RemoveInputAuthority();
                    Runner.Despawn(player.Object);

                    player = disconnectedPlayer;
                    player.Object.AssignInputAuthority(playerRef);
                    player.RefreshPlayerProperties();
                    Runner.SetPlayerAlwaysInterested(playerRef, player.Object, true);
                }

                player.Refresh();
                Runner.SetPlayerObject(playerRef, player.Object);

#if UNITY_EDITOR
                player.gameObject.name = $"Player {player.Nickname}";
#endif

                _gameplayMode.PlayerJoined(player);
            }

            for (int i = 0; i < playersToRemove.Count; i++)
            {
                _pendingPlayers.Remove(playersToRemove[i]);
            }

            ListPool.Return(playersToRemove);
        }

        // IPlayerJoined/IPlayerLeft INTERFACES

        void IPlayerJoined.PlayerJoined(PlayerRef playerRef)
        {
            if (Runner.IsServer == false)
                return;
            if (_isActive == false)
                return;

            SpawnPlayer(playerRef);
        }

        void IPlayerLeft.PlayerLeft(PlayerRef playerRef)
        {
            if (playerRef.IsRealPlayer == false)
                return;
            if (Runner.IsServer == false)
                return;
            if (_isActive == false)
                return;

            OnPlayerLeft(GetPlayer(playerRef));
        }

        private void OnPlayerLeft(Player player)
        {
            if (player == null)
                return;

            ActivePlayers.Remove(player);

            if (player.UserID.HasValue() == true)
            {
                _disconnectedPlayers[player.UserID] = player;

                _gameplayMode.PlayerLeft(player);

                player.Object.RemoveInputAuthority();

#if UNITY_EDITOR
                player.gameObject.name = $"{player.gameObject.name} (Disconnected)";
#endif
            }
            else
            {
                _gameplayMode.PlayerLeft(player);

                // Player wasn't initilized properly, safe to despawn
                Runner.Despawn(player.Object);
            }
        }

        // MonoBehaviour INTERFACE

        private void Update()
        {
            if (ApplicationSettings.RecordSession == false)
                return;
            if (Object == null)
                return;

            if (_statsRecorder == null)
            {
                string fileID = $"{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

                string statsFileName = $"FusionBR_{fileID}_Stats.log";
                string logFileName = $"FusionBR_{fileID}_Log.log";

                _statsRecorder = new StatsRecorder();
                _statsRecorder.Initialize(ApplicationUtility.GetFilePath(statsFileName), fileID, "Time", "Players", "DeltaTime");

                _logRecorder = new LogRecorder();
                _logRecorder.Initialize(ApplicationUtility.GetFilePath(logFileName));
                _logRecorder.Write(fileID);

                Application.logMessageReceived -= OnLogMessage;
                Application.logMessageReceived += OnLogMessage;

                PrintInfo();
            }

            string time = Time.realtimeSinceStartup.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string players = ActivePlayerCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string deltaTime = (Time.deltaTime * 1000.0f).ToString(System.Globalization.CultureInfo.InvariantCulture);

            _statsRecorder.Write(time, players, deltaTime);
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (_logRecorder == null)
                return;

            _logRecorder.Write(condition);

            if (type == LogType.Exception)
            {
                _logRecorder.Write(stackTrace);
            }
        }

        private void OnDestroy()
        {
            if (_statsRecorder != null)
            {
                _statsRecorder.Deinitialize();
                _statsRecorder = null;
            }

            if (_logRecorder != null)
            {
                _logRecorder.Deinitialize();
                _logRecorder = null;
            }
        }

        // PRIVATE METHODS

        private void SpawnPlayer(PlayerRef playerRef)
        {
            if (GetPlayer(playerRef) != null || _pendingPlayers.ContainsKey(playerRef) == true)
            {
                Log.Error($"Player for {playerRef} is already spawned!");
                return;
            }

            var player = Runner.Spawn(_playerPrefab, inputAuthority: playerRef);

            Runner.SetPlayerAlwaysInterested(playerRef, player.Object, true);

            _pendingPlayers[playerRef] = player;

#if UNITY_EDITOR
            player.gameObject.name = $"Player Unknown (Pending)";
#endif
        }

        /// <summary>
        /// Moves all existing player objects from the Fusion snapshot into the disconnected player pool and
        /// removes their input authorities. Called by <see cref="TPSBR.Networking"/> on the new host immediately
        /// after host migration so that reconnecting clients are matched to their original player objects via
        /// the UserID-based reconnection flow in <see cref="FixedUpdateNetwork"/>.
        /// </summary>
        public void PrepareForMigrationResume(NetworkRunner runner)
        {
            // Block win-condition checks until Activate() completes. Without this the departing
            // host's player being removed can drop alivePlayers to 1 and falsely end the match.
            IsMigrating = true;

            _allPlayers.Clear();
            runner.GetAllBehaviours<Player>(_allPlayers);

            int movedCount = 0;
            int skippedCount = 0;

            foreach (var player in _allPlayers)
            {
                if (player == null || player.Object == null)
                {
                    skippedCount++;
                    continue;
                }

                if (player.UserID.HasValue() == true)
                {
                    _disconnectedPlayers[player.UserID] = player;
                    player.Object.RemoveInputAuthority();
                    movedCount++;
                }
                else
                {
                    player.Object.RemoveInputAuthority();
                    skippedCount++;
                    Debug.LogWarning($"[Host Migration] Player '{player.gameObject.name}' has no UserID — input authority cleared; will be despawned if client does not reconnect.");
                }
            }

            _allPlayers.Clear();

            if (runner.SessionInfo != null)
            {
                runner.SessionInfo.IsOpen = true;
                runner.SessionInfo.IsVisible = true;
            }

            Debug.LogWarning($"[Host Migration] PrepareForMigrationResume: {movedCount} player(s) queued for reconnection, {skippedCount} no-UserID player(s) cleared.");
        }

        private void GenerateLevel(int seed)
        {
            if (_isActive == false || _levelGenerator == null || _levelGenerated == true || seed == 0)
                return;

            _levelGenerated = true;

            _levelGenerator.Generate(seed, _levelSize, _areaCount);

            Context.Map.OverrideParameters(_levelGenerator.Center, _levelGenerator.Dimensions);

            _shrinkingArea.OverrideParameters(_levelGenerator.Center, _levelGenerator.Dimensions.x * 0.5f, _levelGenerator.Dimensions.x * 0.5f, 50f);

            int areaOffset = Random.Range(0, _areaCount);
            for (int i = 0; i < _areaCount; i++)
            {
                int areaID = (i + areaOffset) % _areaCount;

                Vector2 shrinkEnd = _levelGenerator.Areas[areaID].Center * _levelGenerator.BlockSize;
                if (_shrinkingArea.SetEndCenter(shrinkEnd, i == _areaCount - 1) == true)
                    break;
            }

            Debug.Log($"Level generated, center: {_levelGenerator.Center}, dimensions: {_levelGenerator.Dimensions}");

            if (HasStateAuthority == false)
                return;

            Debug.Log($"Spawning {_levelGenerator.ObjectsToSpawn.Count} level generated objects");

            for (int i = 0; i < _levelGenerator.ObjectsToSpawn.Count; i++)
            {
                var spawnData = _levelGenerator.ObjectsToSpawn[i];
                var spawnedObject = Runner.Spawn(spawnData.Prefab, spawnData.Position, spawnData.Rotation);

                if (spawnData.IsConnector == true)
                {
                    var connector = spawnedObject.GetComponent<IBlockConnector>();

                    connector.SetMaterial(spawnData.AreaID, spawnData.Material);
                    connector.SetHeight(spawnData.Height);
                }
            }

            if (_maxItemBoxes > 0)
            {
                while (_levelGenerator.ItemBoxesToSpawn.Count > _maxItemBoxes)
                {
                    _levelGenerator.ItemBoxesToSpawn.RemoveBySwap(Random.Range(0, _levelGenerator.ItemBoxesToSpawn.Count));
                }
            }

            Debug.Log($"Spawning {_levelGenerator.ItemBoxesToSpawn.Count} item boxes");

            for (int i = 0; i < _levelGenerator.ItemBoxesToSpawn.Count; i++)
            {
                var spawnData = _levelGenerator.ItemBoxesToSpawn[i];
                var spawnedObject = Runner.Spawn(spawnData.Prefab, spawnData.Position, spawnData.Rotation);
            }
        }

        private void PrintInfo()
        {
            Debug.Log($"ApplicationUtility.DataPath: {ApplicationUtility.DataPath}");
            Debug.Log($"Environment.CommandLine: {Environment.CommandLine}");
            Debug.Log($"SystemInfo.deviceModel: {SystemInfo.deviceModel}");
            Debug.Log($"SystemInfo.deviceName: {SystemInfo.deviceName}");
            Debug.Log($"SystemInfo.deviceType: {SystemInfo.deviceType}");
            Debug.Log($"SystemInfo.processorCount: {SystemInfo.processorCount}");
            Debug.Log($"SystemInfo.processorFrequency: {SystemInfo.processorFrequency}");
            Debug.Log($"SystemInfo.processorType: {SystemInfo.processorType}");
            Debug.Log($"SystemInfo.systemMemorySize: {SystemInfo.systemMemorySize}");
        }

        // NETWORK CALLBACKS

        private void OnHostMigration(NetworkRunner runner, HostMigrationToken token)
        {
            Debug.LogWarning($"[Host Migration] Received token. Becoming {token.GameMode}");

            var request = _sessionRequest;
            request.GameMode = GameMode.Host;
            request.HostMigrationToken = token;

            _sessionRequest.HostMigrationToken = null;

            Global.Networking.StartGame(request);
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            if (runner != null)
            {
                runner.SetLocalPlayer(_localPlayer);
            }
        }
    }
}