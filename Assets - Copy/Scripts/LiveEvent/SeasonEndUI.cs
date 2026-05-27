using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TPSBR
{
    public class SeasonEndUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _exitButtonText;
        [SerializeField] private Image _backgroundImage;
        
        [Header("Settings")]
        [SerializeField] private string _titleMessage = "SEASON ENDED";
        [SerializeField] private string _updateMessage = "Please update your game to continue playing";
        [SerializeField] private string _lobbySceneName = "Menu";
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.95f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip _countdownMusic;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.5f;
        [SerializeField] private bool _loopMusic = true;
        [SerializeField] private float _musicFadeInDuration = 9f;
        
        private double _targetTime;
        private bool _isCountingDown = false;
        private AudioSource _musicSource;
        private Coroutine _musicFadeCoroutine;
        
        private void Awake()
        {
            Debug.Log("[SeasonEndUI] Awake called");
            
            if (_canvas != null)
            {
                _canvas.enabled = false;
                Debug.Log("[SeasonEndUI] Canvas disabled on Awake");
            }
            else
            {
                Debug.LogError("[SeasonEndUI] Canvas is null in Awake!");
            }
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _backgroundColor;
            }
            
            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitButtonClicked);
                _exitButton.interactable = true;
                Debug.Log($"[SeasonEndUI] Exit button hooked up. Interactable: {_exitButton.interactable}");
            }
            else
            {
                Debug.LogError("[SeasonEndUI] Exit button is null in Awake!");
            }
            
            SetupAudioSource();
        }
        
        private void SetupAudioSource()
        {
            _musicSource = gameObject.GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[SeasonEndUI] AudioSource created");
            }
            
            if (_musicSource.isPlaying)
            {
                _musicSource.Stop();
                Debug.Log("[SeasonEndUI] Stopped auto-playing AudioSource");
            }
            
            _musicSource.playOnAwake = false;
            _musicSource.loop = false;
            _musicSource.volume = 0f;
            _musicSource.clip = null;
            _musicSource.enabled = false;
            
            Debug.Log("[SeasonEndUI] AudioSource configured and disabled");
        }
        
        public void ShowCountdown(double secondsUntilNextSeason)
        {
            Debug.Log($"[SeasonEndUI] ShowCountdown called with {secondsUntilNextSeason} seconds");
            
            ShowCursor();
            
            if (_canvas != null)
            {
                _canvas.enabled = true;
                _canvas.sortingOrder = 20000;
                Debug.Log($"[SeasonEndUI] Canvas enabled with sorting order: {_canvas.sortingOrder}");
            }
            else
            {
                Debug.LogError("[SeasonEndUI] Canvas is null!");
            }
            
            _targetTime = DateTime.UtcNow.AddSeconds(secondsUntilNextSeason).Ticks;
            _isCountingDown = true;
            
            if (_titleText != null)
            {
                _titleText.text = _titleMessage;
                Debug.Log($"[SeasonEndUI] Title text set to: {_titleMessage}");
            }
            
            if (_messageText != null)
            {
                _messageText.text = _updateMessage;
            }
            
            if (_countdownText != null)
            {
                Debug.Log("[SeasonEndUI] Starting countdown coroutine");
            }
            else
            {
                Debug.LogError("[SeasonEndUI] Countdown text is null!");
            }
            
            PlayCountdownMusic();
            StartCoroutine(UpdateCountdown());
        }
        
        private void PlayCountdownMusic()
        {
            if (_countdownMusic != null && _musicSource != null)
            {
                if (_musicFadeCoroutine != null)
                {
                    StopCoroutine(_musicFadeCoroutine);
                }
                
                _musicSource.enabled = true;
                _musicSource.clip = _countdownMusic;
                _musicSource.volume = 0f;
                _musicSource.loop = _loopMusic;
                _musicSource.Play();
                
                _musicFadeCoroutine = StartCoroutine(FadeInMusic());
                Debug.Log($"[SeasonEndUI] Playing countdown music: {_countdownMusic.name} (Fading in over {_musicFadeInDuration}s to volume {_musicVolume})");
            }
            else if (_countdownMusic == null)
            {
                Debug.Log("[SeasonEndUI] No countdown music assigned");
            }
        }
        
        private IEnumerator FadeInMusic()
        {
            float elapsed = 0f;
            
            while (elapsed < _musicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / _musicFadeInDuration;
                _musicSource.volume = Mathf.Lerp(0f, _musicVolume, normalizedTime);
                yield return null;
            }
            
            _musicSource.volume = _musicVolume;
            Debug.Log($"[SeasonEndUI] Music fade-in complete. Final volume: {_musicVolume}");
            _musicFadeCoroutine = null;
        }
        
        private void StopCountdownMusic()
        {
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
                _musicFadeCoroutine = null;
            }
            
            if (_musicSource != null)
            {
                if (_musicSource.isPlaying)
                {
                    _musicSource.Stop();
                    Debug.Log("[SeasonEndUI] Countdown music stopped");
                }
                
                _musicSource.volume = 0f;
                _musicSource.clip = null;
                _musicSource.enabled = false;
            }
        }
        
        private IEnumerator UpdateCountdown()
        {
            while (_isCountingDown)
            {
                DateTime targetDateTime = new DateTime((long)_targetTime, DateTimeKind.Utc);
                TimeSpan remaining = targetDateTime - DateTime.UtcNow;
                
                if (remaining.TotalSeconds <= 0)
                {
                    if (_countdownText != null)
                    {
                        _countdownText.text = "00:00:00:00";
                    }
                    
                    if (_messageText != null)
                    {
                        _messageText.text = "New Season is Live! Please update your game.";
                    }
                    
                    _isCountingDown = false;
                    yield break;
                }
                
                if (_countdownText != null)
                {
                    int days = remaining.Days;
                    int hours = remaining.Hours;
                    int minutes = remaining.Minutes;
                    int seconds = remaining.Seconds;
                    
                    _countdownText.text = $"{days:D2}:{hours:D2}:{minutes:D2}:{seconds:D2}";
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void OnExitButtonClicked()
        {
            Debug.Log("[SeasonEndUI] Exit button clicked - returning to lobby");
            Hide();
            ReturnToLobby();
        }
        
        private void ReturnToLobby()
        {
            Debug.Log("[SeasonEndUI] Returning to lobby via Networking.StopGame()");
            
            StopCountdownMusic();
            
            Networking networking = FindAnyObjectByType<Networking>();
            if (networking != null)
            {
                networking.StopGame();
            }
            else
            {
                Debug.LogWarning("[SeasonEndUI] Networking not found! Attempting direct scene load...");
                
                if (Fusion.NetworkRunner.Instances != null)
                {
                    foreach (var runner in Fusion.NetworkRunner.Instances)
                    {
                        if (runner != null && runner.IsRunning)
                        {
                            runner.Shutdown();
                        }
                    }
                }
                
                SceneManager.LoadScene(_lobbySceneName);
            }
        }
        
        public void Hide()
        {
            _isCountingDown = false;
            StopCountdownMusic();
            
            if (_canvas != null)
            {
                _canvas.enabled = false;
            }
        }
        
        private void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("[SeasonEndUI] Cursor shown and unlocked");
        }
        
        private void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("[SeasonEndUI] Cursor hidden and locked");
        }
    }
}
