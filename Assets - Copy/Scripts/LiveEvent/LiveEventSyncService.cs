using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TPSBR
{
    /// <summary>
    /// Polls the backend /liveevent/status endpoint on the Host/Server to keep
    /// LiveEventData in sync with admin-set times and to handle force-triggers.
    /// Attach this component to the same GameObject as LiveEventManager (or any
    /// persistent GameObject). Only the Host/Server polls; clients receive updates
    /// through LiveEventManager's existing networked RPCs.
    /// </summary>
    public class LiveEventSyncService : MonoBehaviour
    {
        [Header("Backend")]
        [Tooltip("Full URL of your backend, e.g. http://localhost:8000")]
        [SerializeField] private string _backendURL = "http://localhost:8000";

        [Header("Polling")]
        [Tooltip("How often (seconds) the Host polls the backend for event changes")]
        [SerializeField] private float _pollIntervalSeconds = 60f;

        [Tooltip("How often (seconds) to check for a force-trigger flag (should be shorter than poll interval)")]
        [SerializeField] private float _triggerPollIntervalSeconds = 10f;

        [Header("Live Event")]
        [Tooltip("The LiveEventData asset whose EventStartTimeEST will be updated from the backend")]
        [SerializeField] private LiveEventData _liveEventData;

        // Last known force-trigger timestamp to avoid re-triggering on repeat polls
        private string _lastKnownForceTriggerTimestamp = string.Empty;
        private bool _isPolling = false;

        private const string STATUS_ENDPOINT = "/liveevent/status";

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

        /// <summary>Starts both poll loops. Safe to call multiple times.</summary>
        public void StartPolling()
        {
            if (_isPolling) return;
            _isPolling = true;

            StartCoroutine(ScheduledTimePollLoop());
            StartCoroutine(ForceTriggerPollLoop());

            Debug.Log($"[LiveEventSyncService] Polling backend at {_backendURL} every {_pollIntervalSeconds}s (trigger check every {_triggerPollIntervalSeconds}s)");
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
        // JSON response model
        // --------------------------------------------------------

        [Serializable]
        private class LiveEventStatus
        {
            public string log;
            public string EventStartTimeEST;
            public bool ForceTriggered;
            public string ForceTriggerTimestamp;
        }
    }
}
