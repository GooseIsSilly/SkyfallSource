using UnityEngine;
using System.Collections.Generic;
using UnitySceneManagement = UnityEngine.SceneManagement;

namespace TPSBR.Backend
{
    public class LoginSceneManager : MonoBehaviour
    {
        [Header("Login UI")]
        [Tooltip("The login panel GameObject that should stay visible")]
        [SerializeField] private GameObject loginPanel;

        [Header("Settings")]
        [Tooltip("If true, automatically hides everything except login on Start")]
        [SerializeField] private bool hideEverythingOnStart = true;

        [Tooltip("GameObjects to never hide (will auto-add BackendManagers and EventSystem)")]
        [SerializeField] private List<GameObject> neverHideObjects = new List<GameObject>();

        private List<GameObject> hiddenObjects = new List<GameObject>();
        private bool isShowingLogin = false;

        private void Awake()
        {
            // Subscribe to login events
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess += OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated += OnLoginSuccess;
            }
            else
            {
                Debug.LogWarning("[LoginSceneManager] BackendServiceManager.Instance is null in Awake!");
            }
        }

        private void OnEnable()
        {
            // Re-subscribe when enabled (in case Instance wasn't ready in Awake)
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess -= OnLoginSuccess; // Remove first to avoid duplicates
                BackendServiceManager.Instance.OnAccountCreated -= OnLoginSuccess;
                
                BackendServiceManager.Instance.OnLoginSuccess += OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated += OnLoginSuccess;
            }
        }

        private void Start()
        {
            if (hideEverythingOnStart)
            {
                if (BackendServiceManager.Instance != null && BackendServiceManager.Instance.HasStoredToken())
                {
                    ShowFeedback("Validating session...");
                    
                    BackendServiceManager.Instance.ValidateToken((success, username) =>
                    {
                        if (success)
                        {
                            ShowFeedback($"Welcome back, {username}!");
                            ShowEverything();
                        }
                        else
                        {
                            ShowFeedback("Session expired. Please login.");
                            HideEverythingExceptLogin();
                        }
                    });
                }
                else
                {
                    HideEverythingExceptLogin();
                }
            }
        }

        public void HideEverythingExceptLogin()
        {
            if (isShowingLogin)
                return;

            Debug.Log("[LoginSceneManager] Hiding everything except login panel...");
            isShowingLogin = true;
            hiddenObjects.Clear();

            UnitySceneManagement.Scene activeScene = UnitySceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                if (ShouldHideObject(obj))
                {
                    if (obj.activeSelf)
                    {
                        hiddenObjects.Add(obj);
                        obj.SetActive(false);
                    }
                }
            }

            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
            }

            Debug.Log($"[LoginSceneManager] Hid {hiddenObjects.Count} objects. Login panel is now visible.");
        }

        public void ShowEverything()
        {
            if (!isShowingLogin)
                return;

            Debug.Log("[LoginSceneManager] Showing all hidden objects...");
            isShowingLogin = false;

            foreach (GameObject obj in hiddenObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            hiddenObjects.Clear();

            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }

            Debug.Log("[LoginSceneManager] All objects shown. Login panel hidden.");
        }

        private bool ShouldHideObject(GameObject obj)
        {
            if (obj == loginPanel)
                return false;

            if (obj.name == "BackendManagers")
                return false;

            if (obj.name == "EventSystem")
                return false;

            if (neverHideObjects.Contains(obj))
                return false;

            return true;
        }

        private void OnLoginSuccess(string username)
        {
            Debug.Log($"[LoginSceneManager] *** OnLoginSuccess EVENT RECEIVED! Username: {username} ***");
            ShowEverything();
        }

        private void ShowFeedback(string message)
        {
            Debug.Log($"[LoginSceneManager] {message}");
        }

        private void OnDestroy()
        {
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated -= OnLoginSuccess;
            }
        }

        private void OnDisable()
        {
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated -= OnLoginSuccess;
            }
        }
    }
}
