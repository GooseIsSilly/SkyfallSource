using UnityEngine;
using Fusion;

namespace TPSBR
{
    /// <summary>
    /// Attaches to the Networking GameObject and forwards runner shutdown events to
    /// <see cref="HostMigrationManager"/> for logging purposes.
    /// The full host migration flow is handled by <see cref="NetworkGame"/>.
    /// </summary>
    [RequireComponent(typeof(Networking))]
    [RequireComponent(typeof(HostMigrationManager))]
    public class NetworkingHostMigrationIntegration : MonoBehaviour
    {
        private HostMigrationManager _hostMigrationManager;

        private void Awake()
        {
            _hostMigrationManager = GetComponent<HostMigrationManager>();
        }
    }
}
