using UnityEngine;
using Fusion;

namespace TPSBR
{
    /// <summary>
    /// Passive observer that logs host-migration-related runner events.
    /// The actual migration logic lives in <see cref="NetworkGame"/>.
    /// </summary>
    public class HostMigrationManager : MonoBehaviour
    {
        private const string LOG_PREFIX = "[Host Migration]";

        /// <summary>Called by NetworkingHostMigrationIntegration to log shutdown events.</summary>
        public void HandleShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            if (reason == ShutdownReason.HostMigration)
            {
                Debug.Log($"{LOG_PREFIX} Runner shutting down for host migration.");
            }
            else
            {
                Debug.LogWarning($"{LOG_PREFIX} Runner shutdown with reason: {reason}. (Not a host migration.)");
            }
        }
    }
}