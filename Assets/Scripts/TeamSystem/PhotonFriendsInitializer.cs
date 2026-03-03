using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class PhotonFriendsInitializer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private string _defaultNickname = "Player";

        private NetworkRunner _runner;
        private bool _initialized = false;

        private void Start()
        {
            _runner = GetComponent<NetworkRunner>();

            if (_runner == null)
            {
                _runner = FindObjectOfType<NetworkRunner>();
            }

            if (_autoInitialize && _runner != null)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (!_initialized && _runner != null && _runner.IsRunning)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (_initialized || _runner == null || !_runner.IsRunning)
                return;

            var partyManager = PartyLobbyManager.Instance;
            if (partyManager == null)
            {
                Debug.LogWarning("[PhotonFriendsInitializer] PartyLobbyManager not found");
                return;
            }

            string userID = GenerateUserID();
            string nickname = _defaultNickname;

            partyManager.Initialize(userID, nickname);
            partyManager.InitializeWithRunner(_runner);

            _initialized = true;
            Debug.Log($"[PhotonFriendsInitializer] Initialized friends system for {userID} ({nickname})");
        }

        private string GenerateUserID()
        {
            if (_runner != null && _runner.IsRunning && _runner.LocalPlayer != PlayerRef.None)
            {
                return $"Player_{_runner.LocalPlayer.PlayerId}";
            }

            return $"Player_{Random.Range(1000, 9999)}";
        }

        public void ManualInitialize(string userID, string nickname)
        {
            if (_runner == null || !_runner.IsRunning)
            {
                Debug.LogWarning("[PhotonFriendsInitializer] NetworkRunner not ready");
                return;
            }

            var partyManager = PartyLobbyManager.Instance;
            if (partyManager != null)
            {
                partyManager.Initialize(userID, nickname);
                partyManager.InitializeWithRunner(_runner);
                _initialized = true;
                Debug.Log($"[PhotonFriendsInitializer] Manually initialized for {userID} ({nickname})");
            }
        }
    }
}
