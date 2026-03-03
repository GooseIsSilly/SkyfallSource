using UnityEngine;

namespace TPSBR.Backend
{
    [CreateAssetMenu(fileName = "BackendConfig", menuName = "TPSBR/Backend/Backend Configuration", order = 1)]
    public class BackendConfig : ScriptableObject
    {
        [Header("Server URLs")]
        [Tooltip("Development backend URL (e.g., http://localhost:8000)")]
        public string developmentURL = "http://localhost:8000";

        [Tooltip("Staging backend URL")]
        public string stagingURL = "";

        [Tooltip("Production backend URL")]
        public string productionURL = "";

        [Header("API Settings")]
        [Tooltip("Timeout for API requests in seconds")]
        [Range(5f, 60f)]
        public float apiTimeout = 10f;

        [Tooltip("Number of retry attempts for failed requests")]
        [Range(1, 5)]
        public int retryAttempts = 3;

        [Header("Ban Check Settings")]
        [Tooltip("Interval between ban status checks in seconds")]
        [Range(10f, 300f)]
        public float banCheckInterval = 30f;

        [Header("Environment")]
        public BackendEnvironment currentEnvironment = BackendEnvironment.Development;

        public string GetCurrentURL()
        {
            switch (currentEnvironment)
            {
                case BackendEnvironment.Development:
                    return developmentURL;
                case BackendEnvironment.Staging:
                    return stagingURL;
                case BackendEnvironment.Production:
                    return productionURL;
                default:
                    return developmentURL;
            }
        }

        public enum BackendEnvironment
        {
            Development,
            Staging,
            Production
        }
    }
}
