using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

namespace TPSBR
{
    public class SeasonEndController : NetworkBehaviour
    {
        public static SeasonEndController Instance { get; private set; }
        
        [Header("Season Configuration")]
        [SerializeField] private string _currentSeasonVersion = "1.0.0";
        [Tooltip("Set countdown end time in format: YYYY-MM-DD HH:MM:SS (UTC time)")]
        [SerializeField] private string _nextSeasonStartTime = "2026-01-30 00:00:00";
        
        [Header("References")]
        [SerializeField] private CanvasGroup _fadeOverlay;
        [SerializeField] private SeasonEndUI _seasonEndUI;
        
        [Header("Settings")]
        [SerializeField] private float _fadeDuration = 5f;
        [SerializeField] private string _lobbySceneName = "Menu";
        [SerializeField] private bool _testMode = false;
        [Tooltip("In test mode, countdown will be this many seconds")]
        [SerializeField] private float _testCountdownSeconds = 120f;
        
        [Networked] private NetworkBool NetworkSeasonEnded { get; set; }
        [Networked] private NetworkBool NetworkCountdownShown { get; set; }
        
        private bool _isSeasonEnded = false;
        private bool _isFading = false;
        private bool _countdownShown = false;
        
        private const string PREF_SEASON_ENDED = "SeasonEndController_SeasonEnded";
        private const string PREF_SEASON_VERSION = "SeasonEndController_SeasonVersion";
        
        public event Action OnSeasonEnded;
        
        public bool IsSeasonActive => !_isSeasonEnded;
        public bool IsInDowntime => _isSeasonEnded && GetSecondsUntilNextSeason() > 0;
        public string CurrentVersion => _currentSeasonVersion;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            LoadSeasonState();
            
            Debug.Log("[SeasonEndController] Initialized");
        }
        
        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                NetworkSeasonEnded = _isSeasonEnded;
                NetworkCountdownShown = _countdownShown;
                Debug.Log($"[SeasonEndController] Server spawned - SeasonEnded: {_isSeasonEnded}");
            }
            else
            {
                _isSeasonEnded = NetworkSeasonEnded;
                _countdownShown = NetworkCountdownShown;
                Debug.Log($"[SeasonEndController] Client spawned - Synced SeasonEnded: {_isSeasonEnded}");
            }
            
            Runner.SetIsSimulated(Object, true);
        }
        
        private void Start()
        {
            CheckSeasonStatus();
            
            if (Object != null && Object.IsValid)
            {
                Debug.Log("[SeasonEndController] Networked mode - waiting for Spawned() callback");
            }
            else
            {
                Debug.LogWarning("[SeasonEndController] Not networked! Running in standalone mode");
            }
            
            if (IsInDowntime)
            {
                Debug.Log("[SeasonEndController] Downtime detected in Start() - showing countdown");
                StartCoroutine(ShowDowntimeCountdown());
            }
            else if (SceneManager.GetActiveScene().name != _lobbySceneName)
            {
                CheckAndHandleDowntime();
            }
        }
        
        private void LoadSeasonState()
        {
            string savedVersion = PlayerPrefs.GetString(PREF_SEASON_VERSION, "");
            
            if (savedVersion != _currentSeasonVersion)
            {
                Debug.Log($"[SeasonEndController] New season version detected! Saved: '{savedVersion}', Current: '{_currentSeasonVersion}'");
                _isSeasonEnded = false;
                PlayerPrefs.DeleteKey(PREF_SEASON_ENDED);
                PlayerPrefs.SetString(PREF_SEASON_VERSION, _currentSeasonVersion);
                PlayerPrefs.Save();
            }
            else
            {
                _isSeasonEnded = PlayerPrefs.GetInt(PREF_SEASON_ENDED, 0) == 1;
                Debug.Log($"[SeasonEndController] Loaded season state: SeasonEnded={_isSeasonEnded}");
            }
        }
        
        private void SaveSeasonState()
        {
            PlayerPrefs.SetInt(PREF_SEASON_ENDED, _isSeasonEnded ? 1 : 0);
            PlayerPrefs.SetString(PREF_SEASON_VERSION, _currentSeasonVersion);
            PlayerPrefs.Save();
            Debug.Log($"[SeasonEndController] Saved season state: SeasonEnded={_isSeasonEnded}");
        }
        
        private void CheckSeasonStatus()
        {
            DateTime nextSeasonTime;
            if (!DateTime.TryParse(_nextSeasonStartTime, out nextSeasonTime))
            {
                Debug.LogError($"[SeasonEndController] Invalid next season time: {_nextSeasonStartTime}");
                return;
            }
            
            DateTime utcNextSeason = DateTime.SpecifyKind(nextSeasonTime, DateTimeKind.Utc);
            TimeSpan timeUntilNextSeason = utcNextSeason - DateTime.UtcNow;
            
            Debug.Log($"[SeasonEndController] Current UTC Time: {DateTime.UtcNow}");
            Debug.Log($"[SeasonEndController] Next Season Time: {utcNextSeason}");
            Debug.Log($"[SeasonEndController] Time Until Next Season: {timeUntilNextSeason.TotalSeconds:F0} seconds");
            
            if (timeUntilNextSeason.TotalSeconds <= 0)
            {
                Debug.Log("[SeasonEndController] Next season has started! Resetting season end state.");
                _isSeasonEnded = false;
                SaveSeasonState();
            }
            else if (_isSeasonEnded)
            {
                Debug.Log($"[SeasonEndController] Season ended, in downtime. {timeUntilNextSeason.Days} days, {timeUntilNextSeason.Hours} hours, {timeUntilNextSeason.Minutes} minutes remaining.");
            }
            else
            {
                Debug.Log($"[SeasonEndController] Season active. Next season in {timeUntilNextSeason.Days} days, {timeUntilNextSeason.Hours} hours, {timeUntilNextSeason.Minutes} minutes.");
            }
        }
        
        public void TriggerSeasonEnd()
        {
            if (_isSeasonEnded && _countdownShown)
            {
                Debug.LogWarning("[SeasonEndController] Season end already triggered and countdown shown!");
                return;
            }
            
            Debug.Log($"[SeasonEndController] TriggerSeasonEnd called! HasStateAuthority: {HasStateAuthority}, Object valid: {Object != null && Object.IsValid}");
            
            _isSeasonEnded = true;
            _countdownShown = true;
            SaveSeasonState();
            
            Debug.Log("[SeasonEndController] Season end triggered!");
            
            OnSeasonEnded?.Invoke();
            
            if (Object != null && Object.IsValid && HasStateAuthority)
            {
                NetworkSeasonEnded = true;
                NetworkCountdownShown = true;
                Debug.Log("[SeasonEndController] Calling RPC to all clients...");
                RPC_TriggerSeasonEndEffects();
            }
            else if (Object == null || !Object.IsValid)
            {
                Debug.LogWarning("[SeasonEndController] NetworkObject not valid, running locally only");
                StartCoroutine(SeasonEndSequence());
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_TriggerSeasonEndEffects()
        {
            Debug.Log($"[SeasonEndController] RPC received! IsServer: {HasStateAuthority}");
            
            if (!HasStateAuthority)
            {
                _isSeasonEnded = true;
                _countdownShown = true;
                OnSeasonEnded?.Invoke();
            }
            
            StartCoroutine(SeasonEndSequence());
        }
        
        private void CheckAndHandleDowntime()
        {
            if (_isSeasonEnded)
            {
                double secondsUntilSeason = GetSecondsUntilNextSeason();
                
                if (secondsUntilSeason > 0)
                {
                    Debug.Log($"[SeasonEndController] Game is in DOWNTIME. {secondsUntilSeason:F0} seconds until next season.");
                    
                    if (!_countdownShown)
                    {
                        Debug.Log("[SeasonEndController] Showing downtime countdown for new game session...");
                        StartCoroutine(ShowDowntimeCountdown());
                    }
                    else
                    {
                        Debug.Log("[SeasonEndController] Countdown already shown - immediately showing UI...");
                        StartCoroutine(ShowImmediateDowntime());
                    }
                }
                else
                {
                    Debug.Log("[SeasonEndController] Next season has started! Resetting season end flag.");
                    _isSeasonEnded = false;
                    _countdownShown = false;
                }
            }
        }
        
        private IEnumerator ShowDowntimeCountdown()
        {
            Debug.Log("[SeasonEndController] Starting downtime sequence with fade...");
            
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FadeToBlack());
            yield return new WaitForSeconds(0.5f);
            
            if (_seasonEndUI != null)
            {
                double secondsUntilSeason = _testMode ? _testCountdownSeconds : GetSecondsUntilNextSeason();
                _seasonEndUI.ShowCountdown(secondsUntilSeason);
                _countdownShown = true;
            }
        }
        
        private IEnumerator ShowImmediateDowntime()
        {
            if (_fadeOverlay != null)
            {
                _fadeOverlay.gameObject.SetActive(true);
                _fadeOverlay.alpha = 1f;
                _fadeOverlay.blocksRaycasts = false;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (_seasonEndUI != null)
            {
                double secondsUntilSeason = _testMode ? _testCountdownSeconds : GetSecondsUntilNextSeason();
                _seasonEndUI.ShowCountdown(secondsUntilSeason);
            }
        }
        
        private IEnumerator SeasonEndSequence()
        {
            Debug.Log("[SeasonEndController] Starting season end sequence...");
            
            yield return new WaitForSeconds(1f);
            
            Debug.Log("[SeasonEndController] Starting fade to black...");
            yield return StartCoroutine(FadeToBlack());
            
            yield return new WaitForSeconds(0.5f);
            
            if (_seasonEndUI != null)
            {
                Debug.Log("[SeasonEndController] Showing countdown UI...");
                double secondsUntilSeason = _testMode ? _testCountdownSeconds : GetSecondsUntilNextSeason();
                _seasonEndUI.ShowCountdown(secondsUntilSeason);
            }
            else
            {
                Debug.LogError("[SeasonEndController] Season End UI is not assigned!");
            }
        }
        
        private IEnumerator FadeToBlack()
        {
            if (_fadeOverlay == null)
            {
                Debug.LogWarning("[SeasonEndController] Fade overlay not assigned!");
                yield break;
            }
            
            _isFading = true;
            _fadeOverlay.gameObject.SetActive(true);
            _fadeOverlay.blocksRaycasts = false;
            
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
                yield return null;
            }
            
            _fadeOverlay.alpha = 1f;
            _isFading = false;
            Debug.Log("[SeasonEndController] Fade complete - Raycasts unblocked");
        }
        
        private double GetSecondsUntilNextSeason()
        {
            if (!DateTime.TryParse(_nextSeasonStartTime, out DateTime nextSeasonTime))
            {
                return 0;
            }
            
            DateTime utcNextSeason = DateTime.SpecifyKind(nextSeasonTime, DateTimeKind.Utc);
            TimeSpan timeUntil = utcNextSeason - DateTime.UtcNow;
            return timeUntil.TotalSeconds;
        }
        
        public bool IsGameplayAllowed()
        {
            return !_isSeasonEnded;
        }
        
        public void BlockGameplayIfSeasonEnded()
        {
            if (_isSeasonEnded)
            {
                Debug.LogWarning("[SeasonEndController] Gameplay blocked - season has ended!");
                ReturnToLobby();
            }
        }
        
        private void ReturnToLobby()
        {
            Debug.Log("[SeasonEndController] Returning to lobby...");
            
            if (_seasonEndUI != null)
            {
                _seasonEndUI.Hide();
            }
            
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0f;
                _fadeOverlay.gameObject.SetActive(false);
            }
            
            SceneManager.LoadScene(_lobbySceneName);
        }
        
        public void ResetForNewSession()
        {
            Debug.Log("[SeasonEndController] Resetting for new game session...");
            _countdownShown = false;
            
            if (_seasonEndUI != null)
            {
                _seasonEndUI.Hide();
            }
            
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0f;
                _fadeOverlay.gameObject.SetActive(false);
            }
        }
        
        public void ResetDowntimeState()
        {
            Debug.Log("[SeasonEndController] Resetting downtime state...");
            _isSeasonEnded = false;
            _countdownShown = false;
            
            if (_seasonEndUI != null)
            {
                _seasonEndUI.Hide();
            }
            
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0f;
                _fadeOverlay.gameObject.SetActive(false);
            }
            
            SaveSeasonState();
            Debug.Log("[SeasonEndController] ✅ Downtime state reset complete!");
        }
    }
}