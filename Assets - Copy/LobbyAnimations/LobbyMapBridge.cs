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

        /// <summary>World position for the camera/menu root.</summary>
        [SerializeField] private Vector3 _cameraWorldPosition = new Vector3(952.5f, 21.6f, 1115.2f);

        /// <summary>World position for the player character.</summary>
        [SerializeField] private Vector3 _characterWorldPosition = new Vector3(952.5f, 21.6f, 1116.7f);

        [Header("Scene References")]
        /// <summary>The /Menu GameObject — parent of SceneCamera and PlayerPreview.</summary>
        [SerializeField] private Transform _menuRoot;

        /// <summary>The PlayerPreview component that spawns the character model.</summary>
        [SerializeField] private PlayerPreview _playerPreview;

        private void Start()
        {
            // Move the entire menu setup (camera) to the camera spot.
            if (_menuRoot != null)
            {
                _menuRoot.position = _cameraWorldPosition;
            }

            // Place the preview character at its specific world position.
            if (_playerPreview != null)
            {
                _playerPreview.transform.position = _characterWorldPosition;
                _playerPreview.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face camera
            }

            // Load background scene additively (no-op if already loaded).
            if (!SceneManager.GetSceneByName(_mapSceneName).isLoaded)
            {
                var op = SceneManager.LoadSceneAsync(_mapSceneName, LoadSceneMode.Additive);
                if (op != null)
                    StartCoroutine(OnMapLoaded(op));
            }
            else
            {
                TriggerPreview();
            }
        }

        private IEnumerator OnMapLoaded(AsyncOperation op)
        {
            yield return op;
            TriggerPreview();
        }

        /// <summary>
        /// Asks PlayerPreview to spawn the agent the player currently has selected.
        /// Falls back to Global.Settings if SceneContext isn't available yet.
        /// </summary>
        private void TriggerPreview()
        {
            if (_playerPreview == null)
                return;

            // Try getting the selected agent ID from the live player data first.
            string agentID = null;

            if (Global.PlayerService != null && Global.PlayerService.PlayerData != null)
            {
                agentID = Global.PlayerService.PlayerData.AgentID;
            }

            // Fallback: pick the first registered agent from AgentSettings.
            if (string.IsNullOrEmpty(agentID) && Global.Settings != null)
            {
                var agents = Global.Settings.Agent.Agents;
                if (agents != null && agents.Length > 0)
                    agentID = agents[0].ID;
            }

            if (!string.IsNullOrEmpty(agentID))
            {
                _playerPreview.ShowAgent(agentID, force: true);
            }
            else
            {
                Debug.LogWarning("[LobbyMapBridge] Could not resolve an agent ID for the player preview.");
            }
        }
    }
}
