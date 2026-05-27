using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Skyfall.Cinematics
{
    public class CinematicCameraManager : MonoBehaviour
    {
        [Header("Cinematic Shots")]
        [Tooltip("List of all cinematic shots")]
        public List<CinematicShot> shots = new List<CinematicShot>();
        
        [Header("Playback Settings")]
        [Tooltip("Auto-play on start")]
        public bool autoPlay = false;
        
        [Tooltip("Loop the sequence")]
        public bool loop = false;
        
        [Tooltip("Blend time between shots")]
        public float blendTime = 1f;
        
        [Header("Controls")]
        [Tooltip("Pause between shots")]
        public bool pauseBetweenShots = false;
        
        private int currentShotIndex = -1;
        private bool isPlaying = false;
        private CinemachineBrain cinemachineBrain;
        
        void Start()
        {
            cinemachineBrain = Camera.main?.GetComponent<CinemachineBrain>();
            
            if (cinemachineBrain == null)
            {
                Debug.LogWarning("No CinemachineBrain found on main camera. Adding one.");
                cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
            }
            
            cinemachineBrain.m_DefaultBlend.m_Time = blendTime;
            
            DeactivateAllShots();
            
            if (autoPlay)
            {
                PlaySequence();
            }
        }
        
        public void PlaySequence()
        {
            if (shots.Count == 0)
            {
                Debug.LogWarning("No cinematic shots available to play.");
                return;
            }
            
            if (!isPlaying)
            {
                StartCoroutine(PlayShotsSequence());
            }
        }
        
        public void StopSequence()
        {
            isPlaying = false;
            StopAllCoroutines();
            DeactivateAllShots();
            currentShotIndex = -1;
        }
        
        public void PlayShot(int index)
        {
            if (index < 0 || index >= shots.Count)
            {
                Debug.LogWarning($"Invalid shot index: {index}");
                return;
            }
            
            DeactivateAllShots();
            shots[index].ActivateShot();
            currentShotIndex = index;
        }
        
        public void PlayShot(string shotName)
        {
            CinematicShot shot = shots.Find(s => s.shotName == shotName);
            if (shot != null)
            {
                int index = shots.IndexOf(shot);
                PlayShot(index);
            }
            else
            {
                Debug.LogWarning($"Shot not found: {shotName}");
            }
        }
        
        public void NextShot()
        {
            if (shots.Count == 0) return;
            
            currentShotIndex = (currentShotIndex + 1) % shots.Count;
            PlayShot(currentShotIndex);
        }
        
        public void PreviousShot()
        {
            if (shots.Count == 0) return;
            
            currentShotIndex--;
            if (currentShotIndex < 0)
            {
                currentShotIndex = shots.Count - 1;
            }
            PlayShot(currentShotIndex);
        }
        
        IEnumerator PlayShotsSequence()
        {
            isPlaying = true;
            
            do
            {
                for (int i = 0; i < shots.Count; i++)
                {
                    if (!isPlaying) yield break;
                    
                    PlayShot(i);
                    
                    yield return new WaitForSeconds(shots[i].duration);
                    
                    if (pauseBetweenShots && i < shots.Count - 1)
                    {
                        yield return new WaitUntil(() => Input.anyKeyDown);
                    }
                }
            }
            while (loop && isPlaying);
            
            isPlaying = false;
            DeactivateAllShots();
        }
        
        void DeactivateAllShots()
        {
            foreach (CinematicShot shot in shots)
            {
                shot.DeactivateShot();
            }
        }
        
        public void SetBlendTime(float time)
        {
            blendTime = time;
            if (cinemachineBrain != null)
            {
                cinemachineBrain.m_DefaultBlend.m_Time = blendTime;
            }
        }
        
        public float GetTotalDuration()
        {
            float total = 0f;
            foreach (CinematicShot shot in shots)
            {
                total += shot.duration;
            }
            return total;
        }
        
        public int GetCurrentShotIndex()
        {
            return currentShotIndex;
        }
        
        public bool IsPlaying()
        {
            return isPlaying;
        }
    }
}
