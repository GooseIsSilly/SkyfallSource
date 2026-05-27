using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace TPSBR
{
    /// <summary>
    /// Manhunt LTM — 10 players, 1 prey vs 9 hunters.
    ///
    /// Flow:
    ///   1. Waiting    — players are frozen in place until MinPlayersToStart (8–10) have joined.
    ///   2. Revealing  — server fires RPC; every client runs a local fade-to-black → show role → fade-in.
    ///                   Server waits RevealDuration seconds before unblocking input.
    ///   3. Active     — normal game: prey survives 15 min, hunters kill prey.
    ///                   Prey kills hunter → morphs for 2–4 min.
    ///                   Last 2 min → prey location leaked every 3 s.
    /// </summary>
    public sealed class ManhuntGameplayMode : GameplayMode
    {
        // ---- Inspector ----

        [Header("Manhunt Settings")]
        [Tooltip("Minimum players before the intro sequence begins (8 or 10).")]
        [SerializeField] private int _minPlayersToStart = 8;
        [Tooltip("Seconds the server waits for the client-side reveal animation before unblocking input.")]
        [SerializeField] private float _revealDuration = 5f;

        // ---- Constants ----

        private const float LocationLeakStart      = 120f; // Last 2 minutes
        private const float MorphDurationMin       = 120f; // 2 minutes
        private const float MorphDurationMax       = 240f; // 4 minutes
        private const float LocationUpdateInterval = 3f;

        // ---- Lobby State ----

        public enum ELobbyState : byte
        {
            Waiting   = 0, // Frozen, waiting for enough players
            Revealing = 1, // Role reveal in progress (input still frozen)
            Active    = 2, // Full gameplay
        }

        // ---- Networked ----

        /// <summary>Current lobby phase. Replicated to all clients.</summary>
        [Networked, HideInInspector]
        public ELobbyState LobbyState { get; private set; }

        /// <summary>While true all agent input is zeroed out on every peer.</summary>
        [Networked, HideInInspector]
        public NetworkBool InputFrozen { get; private set; }

        /// <summary>Network ref of the prey player.</summary>
        [Networked, HideInInspector]
        public PlayerRef PreyRef { get; private set; }

        /// <summary>Whether the prey is currently morphed into a hunter's skin.</summary>
        [Networked, HideInInspector]
        public bool IsMorphed { get; private set; }

        /// <summary>Whether the location-leak phase is active.</summary>
        [Networked, HideInInspector]
        public bool LocationLeakActive { get; private set; }

        /// <summary>Last broadcast prey world position (during leak phase).</summary>
        [Networked, HideInInspector]
        public Vector3 PreyPosition { get; private set; }

        [Networked, HideInInspector]
        private TickTimer _revealTimer { get; set; }

        [Networked, HideInInspector]
        private TickTimer _morphTimer { get; set; }

        // ---- Client-side Events ----

        /// <summary>Fired on all clients when the reveal sequence starts. bool payload: true = local player is prey.</summary>
        public System.Action<bool> OnRoleRevealStarted;

        /// <summary>Fired on all clients when gameplay unlocks after the reveal.</summary>
        public System.Action OnGameplayUnlocked;

        /// <summary>Fired on all clients when the location leak phase begins.</summary>
        public System.Action OnLocationLeakStarted;

        /// <summary>Fired on all clients with updated prey world position (leak phase only).</summary>
        public System.Action<Vector3> OnPreyPositionUpdated;

        /// <summary>Fired on all clients when morph state changes.</summary>
        public System.Action<bool> OnMorphStateChanged;

        // ---- Private ----

        private float _locationUpdateTimer;

        // ---- GameplayMode Interface ----

        protected override void OnActivate()
        {
            // TimeLimit must be set to 900 on the prefab Inspector (15 minutes).
            if (TimeLimit <= 0f)
                Debug.LogWarning("[ManhuntGameplayMode] TimeLimit is 0 — set it to 900 in the Inspector.");

            LobbyState  = ELobbyState.Waiting;
            InputFrozen = true;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (State != EState.Active)
                return;

            if (HasStateAuthority == false)
                return;

            switch (LobbyState)
            {
                case ELobbyState.Waiting:
                    TickWaiting();
                    break;

                case ELobbyState.Revealing:
                    TickRevealing();
                    break;

                case ELobbyState.Active:
                    TickActive();
                    break;
            }
        }

        protected override void TrySpawnAgent(Player player)
        {
            Transform spawnPoint = GetRandomSpawnPoint(30f);
            var position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            var rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            SpawnAgent(player.Object.InputAuthority, position, rotation);
        }

        protected override void AgentDeath(ref PlayerStatistics victimStatistics, ref PlayerStatistics killerStatistics)
        {
            base.AgentDeath(ref victimStatistics, ref killerStatistics);

            if (LobbyState != ELobbyState.Active)
                return;

            bool preyIsVictim = victimStatistics.PlayerRef == PreyRef;
            bool preyIsKiller = killerStatistics.PlayerRef == PreyRef;

            if (preyIsVictim)
            {
                FinishGameplay();
                return;
            }

            if (preyIsKiller)
            {
                var hunterPlayer = Context.NetworkGame.GetPlayer(victimStatistics.PlayerRef);
                if (hunterPlayer != null)
                    StartMorph(hunterPlayer);
            }
        }

        protected override void CheckWinCondition()
        {
            if (LobbyState != ELobbyState.Active)
                return;

            bool preyAlive = false;

            foreach (var player in Context.NetworkGame.ActivePlayers)
            {
                if (player == null)
                    continue;

                var stats = player.Statistics;
                if (stats.IsValid == false || stats.IsEliminated)
                    continue;

                if (player.Object.InputAuthority == PreyRef)
                {
                    preyAlive = true;
                    break;
                }
            }

            if (!preyAlive)
                FinishGameplay();
        }

        // ---- Private Tick Methods ----

        private void TickWaiting()
        {
            int readyCount = 0;
            foreach (var player in Context.NetworkGame.ActivePlayers)
            {
                if (player == null)
                    continue;

                var stats = player.Statistics;
                if (stats.IsValid && !stats.IsEliminated)
                    readyCount++;
            }

            if (readyCount >= _minPlayersToStart)
                StartReveal();
        }

        private void TickRevealing()
        {
            if (_revealTimer.Expired(Runner))
                StartActivePhase();
        }

        private void TickActive()
        {
            // Morph expiry
            if (IsMorphed && _morphTimer.Expired(Runner))
                EndMorph();

            // Location leak
            float remaining = RemainingTime;

            if (!LocationLeakActive && remaining > 0f && remaining <= LocationLeakStart)
            {
                LocationLeakActive = true;
                RPC_LocationLeakStarted();
            }

            if (LocationLeakActive)
            {
                _locationUpdateTimer -= Runner.DeltaTime;
                if (_locationUpdateTimer <= 0f)
                {
                    _locationUpdateTimer = LocationUpdateInterval;
                    BroadcastPreyPosition();
                }
            }
        }

        // ---- Phase Transitions ----

        private void StartReveal()
        {
            LobbyState = ELobbyState.Revealing;

            AssignPreyRole();

            _revealTimer = TickTimer.CreateFromSeconds(Runner, _revealDuration);

            RPC_BeginRoleReveal(PreyRef);
        }

        private void StartActivePhase()
        {
            LobbyState  = ELobbyState.Active;
            InputFrozen = false;

            RPC_GameplayUnlocked();
        }

        // ---- Role Assignment ----

        private void AssignPreyRole()
        {
            var activePlayers = Context.NetworkGame.ActivePlayers;
            if (activePlayers == null || activePlayers.Count == 0)
            {
                Debug.LogError("[ManhuntGameplayMode] No active players when assigning roles.");
                return;
            }

            int preyIndex = Random.Range(0, activePlayers.Count);
            PreyRef = activePlayers[preyIndex].Object.InputAuthority;

            Debug.Log($"[ManhuntGameplayMode] Prey assigned: {PreyRef}");
        }

        // ---- Morph ----

        private void StartMorph(Player hunterPlayer)
        {
            float duration = Random.Range(MorphDurationMin, MorphDurationMax);
            IsMorphed   = true;
            _morphTimer = TickTimer.CreateFromSeconds(Runner, duration);

            var preyPlayer = Context.NetworkGame.GetPlayer(PreyRef);
            if (preyPlayer != null)
            {
                preyPlayer.AgentPrefab = hunterPlayer.AgentPrefab;
                preyPlayer.DespawnAgent();
                TrySpawnAgent(preyPlayer);
            }

            Debug.Log($"[ManhuntGameplayMode] Prey morphed for {duration:F1}s");
            RPC_MorphStateChanged(true);
        }

        private void EndMorph()
        {
            IsMorphed = false;

            var preyPlayer = Context.NetworkGame.GetPlayer(PreyRef);
            if (preyPlayer != null && preyPlayer.Statistics.IsAlive)
            {
                preyPlayer.DespawnAgent();
                TrySpawnAgent(preyPlayer);
            }

            Debug.Log("[ManhuntGameplayMode] Prey morph ended.");
            RPC_MorphStateChanged(false);
        }

        // ---- Location Broadcast ----

        private void BroadcastPreyPosition()
        {
            var preyPlayer = Context.NetworkGame.GetPlayer(PreyRef);
            if (preyPlayer == null)
                return;

            var agent = preyPlayer.ActiveAgent;
            if (agent == null)
                return;

            PreyPosition = agent.transform.position;
            RPC_PreyPositionUpdated(PreyPosition);
        }

        // ---- RPCs ----

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_BeginRoleReveal(PlayerRef preyRef)
        {
            PreyRef    = preyRef;
            LobbyState = ELobbyState.Revealing;

            bool localIsPrey = Runner.LocalPlayer == preyRef;
            OnRoleRevealStarted?.Invoke(localIsPrey);

            Debug.Log($"[ManhuntGameplayMode] Role reveal — local is {(localIsPrey ? "PREY" : "HUNTER")}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_GameplayUnlocked()
        {
            InputFrozen = false;
            LobbyState  = ELobbyState.Active;
            OnGameplayUnlocked?.Invoke();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_LocationLeakStarted()
        {
            LocationLeakActive = true;
            OnLocationLeakStarted?.Invoke();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_MorphStateChanged(bool isMorphed)
        {
            OnMorphStateChanged?.Invoke(isMorphed);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Unreliable)]
        private void RPC_PreyPositionUpdated(Vector3 position)
        {
            PreyPosition = position;
            OnPreyPositionUpdated?.Invoke(position);
        }
    }
}
