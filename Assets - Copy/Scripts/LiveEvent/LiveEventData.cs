using System;
using UnityEngine;

namespace TPSBR
{
    [CreateAssetMenu(fileName = "LiveEvent", menuName = "Skyfall/Live Event Data")]
    public class LiveEventData : ScriptableObject
    {
        [Header("Event Timing")]
        [Tooltip("When the animation triggers (EST format: YYYY-MM-DD HH:MM:SS)")]
        public string EventStartTimeEST = "2024-12-31 15:00:00";
        
        [Tooltip("How many seconds BEFORE trigger to show countdown UI (e.g., 300 = show countdown for last 5 minutes)")]
        public float CountdownDuration = 300f;
        
        [Header("Event Configuration")]
        public string EventName = "Satellite Flyover";
        
        [Tooltip("Animation clip to play when event triggers")]
        public AnimationClip EventAnimation;
        
        [Tooltip("Audio clip to play during event")]
        public AudioClip EventAudio;
        
        [Header("UI Settings")]
        [Tooltip("Show countdown timer UI")]
        public bool ShowCountdownUI = true;
        
        [Tooltip("Only show world countdown when within this many seconds of event (0 = always show). Ex: 604800 = 1 week")]
        public float CountdownVisibilityThreshold = 604800f;
        
        [Tooltip("Seconds before event when text turns red")]
        public float RedTextThreshold = 60f;
        
        [Header("Island Flip (The End Event)")]
        [Tooltip("Enable island flip effect like Fortnite Chapter 2 Season 8")]
        public bool EnableIslandFlip = false;
        
        [Tooltip("Duration of the flip rotation in seconds")]
        public float FlipDuration = 120f;
        
        [Tooltip("Final rotation of the island (X = pitch, Y = yaw, Z = roll)")]
        public Vector3 FlipRotation = new Vector3(180f, 0f, 0f);
        
        [Header("Season End Event")]
        [Tooltip("This event marks the end of the season (triggers season end sequence)")]
        public bool IsSeasonEndEvent = false;
        
        [Tooltip("Delay after event before season end sequence starts (seconds)")]
        public float SeasonEndDelay = 5f;
        
        [Header("Explosion Effects")]
        [Tooltip("Enable explosion effect when event ends")]
        public bool EnableExplosion = false;
        
        [Tooltip("Explosion prefab to spawn (should contain ParticleSystem, AudioSource, or VFX Graph)")]
        public GameObject ExplosionPrefab;
        
        [Tooltip("Explosion sound effect to play at explosion position")]
        public AudioClip ExplosionSound;
        
        [Tooltip("World position where explosion spawns (X, Y, Z coordinates)")]
        public Vector3 ExplosionPosition = Vector3.zero;
        
        [Tooltip("Delay after event audio before explosion triggers (seconds)")]
        public float ExplosionDelay = 0f;
        
        [Tooltip("How long the explosion prefab stays before being destroyed (0 = never destroy)")]
        public float ExplosionLifetime = 5f;
        
        private static TimeZoneInfo EasternTimeZone
        {
            get
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                }
            }
        }
        
        public static DateTime GetCurrentESTTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);
        }
        
        public DateTime GetEventTriggerTime()
        {
            if (DateTime.TryParse(EventStartTimeEST, out DateTime result))
            {
                DateTime estTime = DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
                DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(estTime, EasternTimeZone);
                return utcTime;
            }
            
            Debug.LogError($"Failed to parse EventStartTimeEST: {EventStartTimeEST}");
            return DateTime.UtcNow.AddMinutes(5);
        }
        
        public double GetSecondsUntilEvent()
        {
            DateTime triggerTime = GetEventTriggerTime();
            TimeSpan timeUntil = triggerTime - DateTime.UtcNow;
            return timeUntil.TotalSeconds;
        }
    }
}
