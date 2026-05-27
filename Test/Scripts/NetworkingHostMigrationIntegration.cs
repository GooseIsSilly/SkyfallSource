using UnityEngine;
using Fusion;

namespace TPSBR
{
    [RequireComponent(typeof(Networking))]
    [RequireComponent(typeof(HostMigrationManager))]
    public class NetworkingHostMigrationIntegration : MonoBehaviour
    {
        private HostMigrationManager _hostMigrationManager;
        
        private void Awake()
        {
            _hostMigrationManager = GetComponent<HostMigrationManager>();
        }
        
        private void Start()
        {
            SetupHostMigrationCallbacks();
        }
        
        private void SetupHostMigrationCallbacks()
        {
            var networkGame = FindFirstObjectByType<NetworkGame>();
            
            if (networkGame != null)
            {
                AttachToNetworkGame(networkGame);
            }
            else
            {
                Debug.LogWarning("[Host Migration] NetworkGame not found in scene. Will attempt to attach callbacks when game starts.");
            }
        }
        
        public void AttachToNetworkGame(NetworkGame networkGame)
        {
            Debug.LogWarning("[Host Migration] Direct FusionCallbacksHandler access is not possible. Host migration needs to be implemented differently.");
        }
        
        private void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log($"[Host Migration] Host migration triggered. New GameMode will be: {hostMigrationToken.GameMode}");
            
            if (_hostMigrationManager != null)
            {
                _hostMigrationManager.HandleHostMigration(runner, hostMigrationToken);
            }
            else
            {
                Debug.LogError("[Host Migration] HostMigrationManager is null!");
            }
        }
        
        private void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReason == ShutdownReason.HostMigration)
            {
                Debug.Log("[Host Migration] Runner shutting down for host migration.");
            }
        }
        
        private void OnDestroy()
        {
        }
    }
}
