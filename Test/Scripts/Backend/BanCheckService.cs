using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TPSBR.Backend
{
    public class BanCheckService : MonoBehaviour
    {
        public static BanCheckService Instance { get; private set; }

        [Header("Ban Check Settings")]
        [SerializeField] private float banCheckInterval = 30f;
        [SerializeField] private bool checkOnStart = true;

        [Header("Events")]
        public UnityEvent<string, string> OnPlayerBanned;

        private Coroutine _banCheckCoroutine;
        private bool _isChecking = false;
        private DateTime _lastCheckTime;
        private bool _isBanned = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (checkOnStart && BackendServiceManager.Instance != null && BackendServiceManager.Instance.HasStoredToken())
            {
                StartBanChecking();
            }
        }

        public void StartBanChecking()
        {
            if (_banCheckCoroutine != null)
            {
                StopCoroutine(_banCheckCoroutine);
            }

            _isChecking = true;
            _banCheckCoroutine = StartCoroutine(BanCheckLoop());
        }

        public void StopBanChecking()
        {
            if (_banCheckCoroutine != null)
            {
                StopCoroutine(_banCheckCoroutine);
                _banCheckCoroutine = null;
            }

            _isChecking = false;
        }

        public void CheckBanStatusNow()
        {
            if (!_isChecking)
            {
                StartCoroutine(PerformBanCheck());
            }
        }

        private IEnumerator BanCheckLoop()
        {
            while (_isChecking)
            {
                yield return PerformBanCheck();
                yield return new WaitForSeconds(banCheckInterval);
            }
        }

        private IEnumerator PerformBanCheck()
        {
            if (BackendServiceManager.Instance == null)
            {
                yield break;
            }

            bool checkComplete = false;

            BackendServiceManager.Instance.CheckBanStatus((isBanned, reason, expiresAt) =>
            {
                if (isBanned && !_isBanned)
                {
                    _isBanned = true;
                    HandlePlayerBanned(reason, expiresAt);
                }
                else if (!isBanned)
                {
                    _isBanned = false;
                }

                checkComplete = true;
            });

            while (!checkComplete)
            {
                yield return null;
            }

            _lastCheckTime = DateTime.Now;
        }

        private void HandlePlayerBanned(string reason, string expiresAt)
        {
            Debug.LogWarning($"Player has been banned! Reason: {reason}");

            OnPlayerBanned?.Invoke(reason, expiresAt);

            StopBanChecking();
        }

        public bool IsBanned()
        {
            return _isBanned;
        }

        public DateTime GetLastCheckTime()
        {
            return _lastCheckTime;
        }

        private void OnDestroy()
        {
            StopBanChecking();
        }
    }
}
