using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSBR
{
    /// <summary>
    /// Moves the menu root (camera + player preview) to a specific world position
    /// on the map, loads the background scene additively, then triggers the
    /// PlayerPreview to spawn the locally selected agent.
    /// </summary>
    public class LobbyMapBridge : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private string _mapSceneName = "LobbyBackground";

        [Header("Reference Object Names")]
        [Tooltip("The name of the object to find in the scene to use as the camera's position.")]
        [SerializeField] private string _cameraReferenceName = "LobbyCameraPos";
        
        [Tooltip("The name of the object to find in the scene to use as the player's position.")]
        [SerializeField] private string _characterReferenceName = "LobbyPlayerPos";

        [Header("Fallback Manual Positions")]
        [SerializeField] private Vector3 _cameraWorldPosition = new Vector3(952.5f, 21.6f, 1115.2f);
        [SerializeField] private Vector3 _characterWorldPosition = new Vector3(952.5f, 21.6f, 1116.7f);

        [Header("Scene References")]
        [SerializeField] private Transform _menuRoot;
        [SerializeField] private PlayerPreview _playerPreview;
        [SerializeField] private GameObject[] _objectsToHide;

        private void Start()
        {
            if (_objectsToHide != null)
            {
                foreach (var obj in _objectsToHide)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            if (!SceneManager.GetSceneByName(_mapSceneName).isLoaded)
            {
                var op = SceneManager.LoadSceneAsync(_mapSceneName, LoadSceneMode.Additive);
                if (op != null)
                    StartCoroutine(OnMapLoaded(op));
            }
            else
            {
                ApplyPositions();
            }
        }

        private IEnumerator OnMapLoaded(AsyncOperation op)
        {
            yield return op;

            // Use full namespace for Scene to avoid conflict with TPSBR.Scene
            UnityEngine.SceneManagement.Scene backgroundScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_mapSceneName);
            if (backgroundScene.IsValid())
            {
                foreach (GameObject go in backgroundScene.GetRootGameObjects())
                {
                    foreach (var cam in go.GetComponentsInChildren<Camera>(true)) cam.enabled = false;
                    foreach (var listener in go.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
                }
            }

            ApplyPositions();
        }

        private void ApplyPositions()
        {
            // Find objects by name since they are in a different scene
            GameObject camRef = GameObject.Find(_cameraReferenceName);
            GameObject charRef = GameObject.Find(_characterReferenceName);

            if (_menuRoot != null)
            {
                // Ensure physics are disabled so the camera "floats" at the marker
                var rb = _menuRoot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                if (camRef != null)
                {
                    _menuRoot.position = camRef.transform.position;
                    _menuRoot.rotation = camRef.transform.rotation;
                }
                else
                {
                    _menuRoot.position = _cameraWorldPosition;
                    _menuRoot.rotation = Quaternion.identity;
                }

                var sceneCamera = _menuRoot.Find("SceneCamera");
                if (sceneCamera != null)
                {
                    sceneCamera.localPosition = Vector3.zero;
                    sceneCamera.localRotation = Quaternion.identity;

                    var camRb = sceneCamera.GetComponent<Rigidbody>();
                    if (camRb != null)
                    {
                        camRb.isKinematic = true;
                        camRb.useGravity = false;
                    }
                }
            }

            if (_playerPreview != null)
            {
                if (charRef != null)
                {
                    _playerPreview.transform.position = charRef.transform.position;
                    _playerPreview.transform.rotation = charRef.transform.rotation;
                }
                else
                {
                    _playerPreview.transform.position = _characterWorldPosition;
                    _playerPreview.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }

            TriggerPreview();
        }

        private void OnDestroy()
        {
            if (SceneManager.GetSceneByName(_mapSceneName).isLoaded)
            {
                SceneManager.UnloadSceneAsync(_mapSceneName);
            }
        }

        private void TriggerPreview()
        {
            if (_playerPreview == null) return;

            string agentID = null;
            if (Global.PlayerService != null && Global.PlayerService.PlayerData != null)
                agentID = Global.PlayerService.PlayerData.AgentID;

            if (string.IsNullOrEmpty(agentID) && Global.Settings != null)
            {
                var agents = Global.Settings.Agent.Agents;
                if (agents != null && agents.Length > 0) agentID = agents[0].ID;
            }

            if (!string.IsNullOrEmpty(agentID))
                _playerPreview.ShowAgent(agentID, force: true);
        }
    }
}
