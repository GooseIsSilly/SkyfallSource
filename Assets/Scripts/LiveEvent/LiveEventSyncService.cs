using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TPSBR
{
    /// <summary>
    /// Polls the backend /liveevent/status endpoint on the Host/Server to keep
    /// LiveEventData in sync with admin-set times and to handle force-triggers.
    /// Also polls /status/play to detect server downtime and kick all players
    /// back to the main menu when downtime is enabled by the admin.
    /// Attach this component to the same GameObject as LiveEventManager (or any
    /// persistent GameObject). Only the Host/Server polls; clients receive updates
    /// through LiveEventManager's existing networked RPCs.
    /// </summary>
    public class LiveEventSyncService : MonoBehaviour
    {
        [Header("Backend")]
        [Tooltip("Full URL of your backend, e.g. http://localhost:8000")]
        [SerializeField] private string _backendURL = "http://198.50.250.196:5000";

        [Header("Polling")]
        [Tooltip("How often (seconds) the Host polls the backend for event changes")]
        [SerializeField] private float _pollIntervalSeconds = 60f;

        [Tooltip("How often (seconds) to check for a force-trigger flag (should be shorter than poll interval)")]
        [SerializeField] private float _triggerPollIntervalSeconds = 10f;

        [Tooltip("How often (seconds) all clients check the backend for server downtime")]
        [SerializeField] private float _downtimePollIntervalSeconds = 15f;

        [Header("Live Event")]
        [Tooltip("The LiveEventData asset whose EventStartTimeEST will be updated from the backend")]
        [SerializeField] private LiveEventData _liveEventData;

        // Last known force-trigger timestamp to avoid re-triggering on repeat polls
        private string _lastKnownForceTriggerTimestamp = string.Empty;
        private bool _isPolling = false;
        private bool _lastKnownSeasonEndedEnabled = false;
        private bool _seasonEndedInitialized = false;

        private const string STATUS_ENDPOINT = "/liveevent/status";
        private const string DOWNTIME_ENDPOINT = "/status/play";

        private void Start()
        {
            if (_liveEventData == null)
            {
                Debug.LogError("[LiveEventSyncService] No LiveEventData assigned — sync disabled.");
                return;
            }

            StartPolling();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>Starts all poll loops. Safe to call multiple times.</summary>
        public void StartPolling()
        {
            if (_isPolling) return;
            _isPolling = true;

            StartCoroutine(ScheduledTimePollLoop());
            StartCoroutine(ForceTriggerPollLoop());
            StartCoroutine(DowntimePollLoop());
            StartCoroutine(SeasonEndedPollLoop());

            Debug.Log($"[LiveEventSyncService] Polling backend at {_backendURL} every {_pollIntervalSeconds}s " +
                      $"(trigger check every {_triggerPollIntervalSeconds}s, downtime check every {_downtimePollIntervalSeconds}s)");
        }

        // Polls on a longer interval to sync the scheduled event time.
        private IEnumerator ScheduledTimePollLoop()
        {
            while (true)
            {
                yield return FetchStatus(onResult: (status) =>
                {
                    SyncEventTime(status.EventStartTimeEST);
                });

                yield return new WaitForSeconds(_pollIntervalSeconds);
            }
        }

        // Polls on a shorter interval specifically to detect admin force-triggers.
        private IEnumerator ForceTriggerPollLoop()
        {
            // Offset from the scheduled poll so they don't fire simultaneously
            yield return new WaitForSeconds(_triggerPollIntervalSeconds * 0.5f);

            while (true)
            {
                yield return FetchStatus(onResult: (status) =>
                {
                    if (status.ForceTriggered && status.ForceTriggerTimestamp != _lastKnownForceTriggerTimestamp)
                    {
                        _lastKnownForceTriggerTimestamp = status.ForceTriggerTimestamp;
                        HandleForceTrigger();
                    }
                });

                yield return new WaitForSeconds(_triggerPollIntervalSeconds);
            }
        }

        // Polls for the SeasonEndedEnabled flag set by the admin dashboard.
        private IEnumerator SeasonEndedPollLoop()
        {
            // Short initial delay — fires before the 5-second fade completes,
            // so the backend is the authority over any stale local PlayerPrefs state.
            yield return new WaitForSeconds(1f);

            while (true)
            {
                yield return FetchStatus(onResult: (status) =>
                {
                    bool serverEnabled = status.SeasonEndedEnabled;

                    if (serverEnabled == _lastKnownSeasonEndedEnabled && _seasonEndedInitialized)
                        return;

                    bool wasInitialized = _seasonEndedInitialized;
                    _lastKnownSeasonEndedEnabled = serverEnabled;
                    _seasonEndedInitialized = true;

                    if (serverEnabled)
                    {
                        // Admin has season end enabled — trigger it (guard inside prevents double-trigger)
                        Debug.Log("[LiveEventSyncService] Season Ended screen enabled by admin. Triggering season end.");
                        if (SeasonEndController.Instance != null)
                        {
                            SeasonEndController.Instance.TriggerSeasonEnd();
                        }
                        else
                        {
                            Debug.LogWarning("[LiveEventSyncService] SeasonEndController.Instance is null — cannot trigger season end.");
                        }
                    }
                    else
                    {
                        // Admin has season end disabled — reset regardless of prior state
                        // so stale PlayerPrefs can't keep the screen showing after an admin disable.
                        Debug.Log("[LiveEventSyncService] Season Ended screen disabled by admin. Resetting downtime state.");
                        if (SeasonEndController.Instance != null)
                        {
                            SeasonEndController.Instance.ResetDowntimeState();
                        }
                        else
                        {
                            Debug.LogWarning("[LiveEventSyncService] SeasonEndController.Instance is null — cannot reset downtime.");
                        }
                    }
                });

                yield return new WaitForSeconds(_triggerPollIntervalSeconds);
            }
        }

        private IEnumerator FetchStatus(Action<LiveEventStatus> onResult)
        {
            string url = _backendURL.TrimEnd('/') + STATUS_ENDPOINT;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[LiveEventSyncService] Poll failed: {request.error}");
                yield break;
            }

            LiveEventStatus status = null;
            try
            {
                status = JsonUtility.FromJson<LiveEventStatus>(request.downloadHandler.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LiveEventSyncService] Failed to parse status response: {ex.Message}");
                yield break;
            }

            if (status == null || status.log != "ok")
            {
                Debug.LogWarning($"[LiveEventSyncService] Backend returned non-ok status: {request.downloadHandler.text}");
                yield break;
            }

            onResult?.Invoke(status);
        }

        private void SyncEventTime(string eventStartTimeEST)
        {
            if (string.IsNullOrEmpty(eventStartTimeEST))
                return;

            if (_liveEventData.EventStartTimeEST == eventStartTimeEST)
                return;

            Debug.Log($"[LiveEventSyncService] Updating event time from '{_liveEventData.EventStartTimeEST}' → '{eventStartTimeEST}'");
            _liveEventData.EventStartTimeEST = eventStartTimeEST;

            // If LiveEventManager is active, notify it to re-schedule with the new time.
            if (LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.RescheduleFromData(_liveEventData);
            }
        }

        private void HandleForceTrigger()
        {
            Debug.Log($"[LiveEventSyncService] Force-trigger received from backend at {_lastKnownForceTriggerTimestamp}!");

            if (LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.ForceTriggerEvent();
            }
            else
            {
                Debug.LogWarning("[LiveEventSyncService] LiveEventManager.Instance is null — cannot force trigger.");
            }
        }

        // --------------------------------------------------------
        // Downtime polling — runs on every client, not just the host
        // --------------------------------------------------------

        private IEnumerator DowntimePollLoop()
        {
            // Stagger the first check slightly so it doesn't collide with other polls
            yield return new WaitForSeconds(3f);

            while (true)
            {
                yield return FetchDowntimeStatus(onResult: (status) =>
                {
                    if (status.downtimeEnabled)
                    {
                        HandleDowntime(status.message);
                    }
                });

                yield return new WaitForSeconds(_downtimePollIntervalSeconds);
            }
        }

        private IEnumerator FetchDowntimeStatus(Action<DowntimeStatus> onResult)
        {
            string url = _backendURL.TrimEnd('/') + DOWNTIME_ENDPOINT;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[LiveEventSyncService] Downtime poll failed: {request.error}");
                yield break;
            }

            DowntimeStatus status = null;
            try
            {
                status = JsonUtility.FromJson<DowntimeStatus>(request.downloadHandler.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LiveEventSyncService] Failed to parse downtime response: {ex.Message}");
                yield break;
            }

            if (status == null || status.log != "ok")
            {
                Debug.LogWarning($"[LiveEventSyncService] Backend returned non-ok downtime status: {request.downloadHandler.text}");
                yield break;
            }

            onResult?.Invoke(status);
        }

        /// <summary>
        /// In a packaged build, force-quits the application immediately.
        /// In the editor, shuts down the current session and returns to the main menu
        /// so the editor doesn't need a full restart during development.
        /// </summary>
        private void HandleDowntime(string message)
        {
            Debug.Log($"[LiveEventSyncService] Downtime active — kicking player. Message: \"{message}\"");

            // Stop all polling so this doesn't fire again while shutdown is in progress
            StopAllCoroutines();
            _isPolling = false;

#if !UNITY_EDITOR
            Application.Quit();
#else
            string reason = string.IsNullOrWhiteSpace(message)
                ? "Servers are currently under maintenance. Please try again later."
                : message;

            Networking networking = FindAnyObjectByType<Networking>();
            if (networking != null)
            {
                networking.StopGame(reason);
            }
            else
            {
                Debug.LogWarning("[LiveEventSyncService] Networking not found — shutting down runners directly.");

                if (Fusion.NetworkRunner.Instances != null)
                {
                    foreach (Fusion.NetworkRunner runner in Fusion.NetworkRunner.Instances)
                    {
                        if (runner != null && runner.IsRunning)
                        {
                            runner.Shutdown();
                        }
                    }
                }
            }
#endif
        }

        // --------------------------------------------------------
        // JSON response models
        // --------------------------------------------------------

        [Serializable]
        private class LiveEventStatus
        {
            public string log;
            public string EventStartTimeEST;
            public bool ForceTriggered;
            public string ForceTriggerTimestamp;
            public bool SeasonEndedEnabled;
        }

        [Serializable]
        private class DowntimeStatus
        {
            public string log;
            public bool downtimeEnabled;
            public string message;
        }
    }
}
