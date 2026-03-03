using System.Collections;
using UnityEngine;

namespace TPSBR
{
    public class CinematicAudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class MusicTrack
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
        }

        [Header("Music Tracks")]
        [SerializeField] private MusicTrack[] _musicTracks;

        [Header("Settings")]
        [SerializeField] private float _defaultFadeDuration = 2f;
        [SerializeField] private AudioSource _musicSource;

        private Coroutine _currentFadeCoroutine;

        private void Awake()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
            }
        }

        public void PlayMusic(string trackName, float fadeDuration = -1f)
        {
            MusicTrack track = GetTrack(trackName);
            if (track != null && track.clip != null)
            {
                PlayMusic(track.clip, track.volume, fadeDuration >= 0 ? fadeDuration : _defaultFadeDuration);
            }
        }

        public void PlayMusic(AudioClip clip, float targetVolume = 1f, float fadeDuration = -1f)
        {
            if (clip == null)
                return;

            if (fadeDuration < 0)
                fadeDuration = _defaultFadeDuration;

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = StartCoroutine(FadeToNewTrack(clip, targetVolume, fadeDuration));
        }

        public void StopMusic(float fadeDuration = -1f)
        {
            if (fadeDuration < 0)
                fadeDuration = _defaultFadeDuration;

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = StartCoroutine(FadeOut(fadeDuration));
        }

        public void CrossfadeMusic(string trackName, float fadeDuration = -1f)
        {
            MusicTrack track = GetTrack(trackName);
            if (track != null && track.clip != null)
            {
                CrossfadeMusic(track.clip, track.volume, fadeDuration >= 0 ? fadeDuration : _defaultFadeDuration);
            }
        }

        public void CrossfadeMusic(AudioClip newClip, float targetVolume = 1f, float fadeDuration = -1f)
        {
            if (newClip == null)
                return;

            if (fadeDuration < 0)
                fadeDuration = _defaultFadeDuration;

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = StartCoroutine(Crossfade(newClip, targetVolume, fadeDuration));
        }

        public void SetVolume(float volume, float fadeDuration = 0f)
        {
            if (fadeDuration <= 0f)
            {
                _musicSource.volume = volume;
            }
            else
            {
                if (_currentFadeCoroutine != null)
                {
                    StopCoroutine(_currentFadeCoroutine);
                }
                _currentFadeCoroutine = StartCoroutine(FadeVolume(volume, fadeDuration));
            }
        }

        public bool IsPlaying()
        {
            return _musicSource != null && _musicSource.isPlaying;
        }

        public AudioClip GetCurrentClip()
        {
            return _musicSource != null ? _musicSource.clip : null;
        }

        private MusicTrack GetTrack(string trackName)
        {
            foreach (var track in _musicTracks)
            {
                if (track.name == trackName)
                {
                    return track;
                }
            }
            return null;
        }

        private IEnumerator FadeToNewTrack(AudioClip clip, float targetVolume, float duration)
        {
            _musicSource.clip = clip;
            _musicSource.volume = 0f;
            _musicSource.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }

        private IEnumerator FadeOut(float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _musicSource.volume = 0f;
            _musicSource.Stop();
            _currentFadeCoroutine = null;
        }

        private IEnumerator Crossfade(AudioClip newClip, float targetVolume, float duration)
        {
            float halfDuration = duration * 0.5f;
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
                yield return null;
            }

            _musicSource.clip = newClip;
            _musicSource.Play();

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / halfDuration);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }

        private IEnumerator FadeVolume(float targetVolume, float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }
    }
}
