using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSBR
{
    public class QuestSystemInitializer : MonoBehaviour
    {
        [Header("Auto-Initialization")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _persistAcrossScenes = true;

        private static QuestSystemInitializer _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_initializeOnAwake)
            {
                InitializeQuestSystem();
            }
        }

        public void InitializeQuestSystem()
        {
            EnsureQuestManager();
            EnsureCloudCoinManager();
            EnsureQuestEventIntegration();

            Debug.Log("[Quest System] Initialization complete!");
        }

        private void EnsureQuestManager()
        {
            if (QuestManager.Instance == null)
            {
                GameObject questManagerObj = new GameObject("QuestManager");
                questManagerObj.AddComponent<QuestManager>();
                
                if (_persistAcrossScenes)
                {
                    DontDestroyOnLoad(questManagerObj);
                }
                
                Debug.Log("[Quest System] QuestManager created");
            }
        }

        private void EnsureCloudCoinManager()
        {
            if (CloudCoinManager.Instance == null)
            {
                GameObject coinManagerObj = new GameObject("CloudCoinManager");
                coinManagerObj.AddComponent<CloudCoinManager>();
                
                if (_persistAcrossScenes)
                {
                    DontDestroyOnLoad(coinManagerObj);
                }
                
                Debug.Log("[Quest System] CloudCoinManager created");
            }
        }

        private void EnsureQuestEventIntegration()
        {
            if (QuestEventIntegration.Instance == null)
            {
                GameObject integrationObj = new GameObject("QuestEventIntegration");
                integrationObj.AddComponent<QuestEventIntegration>();
                
                if (_persistAcrossScenes)
                {
                    DontDestroyOnLoad(integrationObj);
                }
                
                Debug.Log("[Quest System] QuestEventIntegration created");
            }
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
        }

        [ContextMenu("Force Initialize Quest System")]
        public void ForceInitialize()
        {
            InitializeQuestSystem();
        }

        [ContextMenu("Test Add Coins")]
        public void TestAddCoins()
        {
            if (CloudCoinManager.Instance != null)
            {
                CloudCoinManager.Instance.AddCoins(100);
                Debug.Log($"Added 100 coins. Total: {CloudCoinManager.Instance.GetCurrentCoins()}");
            }
        }

        [ContextMenu("Test Complete Quest")]
        public void TestCompleteQuest()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.UpdateQuestProgress(QuestRequirementType.PlayMatches, 1);
                Debug.Log("Simulated completing 'Play 1 Match' quest");
            }
        }
    }
}
