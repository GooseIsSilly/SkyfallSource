using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class HostMigrationManager : MonoBehaviour
    {
        private const string HOST_MIGRATION_LOG_PREFIX = "[Host Migration]";
        
        private Networking _networking;
        
        private void Awake()
        {
            _networking = GetComponent<Networking>();
        }
        
        public void HandleHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.LogWarning($"================== HOST MIGRATION TRIGGERED ==================");
            Debug.LogWarning($"{HOST_MIGRATION_LOG_PREFIX} Received on this peer");
            Debug.LogWarning($"{HOST_MIGRATION_LOG_PREFIX} Token GameMode: {hostMigrationToken.GameMode}");
            Debug.LogWarning($"{HOST_MIGRATION_LOG_PREFIX} Current Runner IsServer: {runner.IsServer}");
            Debug.LogWarning($"{HOST_MIGRATION_LOG_PREFIX} Current Runner IsClient: {runner.IsClient}");
            Debug.LogWarning($"{HOST_MIGRATION_LOG_PREFIX} Current Runner IsRunning: {runner.IsRunning}");
            Debug.LogWarning($"=============================================================");
            
            Debug.LogError($"{HOST_MIGRATION_LOG_PREFIX} Host migration is NOT fully implemented!");
            Debug.LogError($"{HOST_MIGRATION_LOG_PREFIX} This requires integration with Networking.cs Session system.");
            Debug.LogError($"{HOST_MIGRATION_LOG_PREFIX} Clients will freeze and disconnect without full implementation.");
        }
        
        public void HandleShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            if (reason == ShutdownReason.HostMigration)
            {
                Debug.Log($"{HOST_MIGRATION_LOG_PREFIX} Runner shutdown due to host migration.");
            }
        }
    }
}
