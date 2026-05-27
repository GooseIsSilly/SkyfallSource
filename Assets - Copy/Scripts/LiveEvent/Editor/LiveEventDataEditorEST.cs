using UnityEngine;
using UnityEditor;
using System;

namespace TPSBR
{
    [CustomEditor(typeof(LiveEventData))]
    public class LiveEventDataEditorEST : UnityEditor.Editor
    {
        private const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private int customMinutes = 10;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Event Timeline", EditorStyles.boldLabel);
            
            LiveEventData eventData = (LiveEventData)target;
            
            DateTime triggerTimeUtc = eventData.GetEventTriggerTime();
            DateTime nowEst = LiveEventData.GetCurrentESTTime();
            DateTime nowUtc = DateTime.UtcNow;
            TimeSpan timeUntilTrigger = triggerTimeUtc - nowUtc;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Current EST Time:", nowEst.ToString(TIME_FORMAT));
            EditorGUILayout.LabelField("Animation Triggers At (UTC):", triggerTimeUtc.ToString(TIME_FORMAT));
            
            if (timeUntilTrigger.TotalSeconds > 0)
            {
                EditorGUILayout.LabelField("Time Until Trigger:", FormatTimeSpan(timeUntilTrigger), new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.green } });
                
                if (timeUntilTrigger.TotalSeconds <= eventData.CountdownDuration)
                {
                    EditorGUILayout.LabelField("Countdown Status:", "ACTIVE (countdown visible)", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } });
                }
                else
                {
                    TimeSpan untilCountdownStarts = TimeSpan.FromSeconds(timeUntilTrigger.TotalSeconds - eventData.CountdownDuration);
                    EditorGUILayout.LabelField("Countdown Starts In:", FormatTimeSpan(untilCountdownStarts));
                }
            }
            else
            {
                EditorGUILayout.LabelField("Event Status:", "TRIGGERED", new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.red } });
                EditorGUILayout.LabelField("Time Since Trigger:", FormatTimeSpan(timeUntilTrigger.Negate()));
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"Countdown shows for the last {eventData.CountdownDuration} seconds before the animation triggers.\n" +
                "Set 'Event Start Time EST' to when you want the animation to play (in EST/EDT timezone).", 
                MessageType.Info
            );
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Trigger in 1 Minute"))
            {
                SetTriggerTime(1);
            }
            
            if (GUILayout.Button("Trigger in 3 Minutes"))
            {
                SetTriggerTime(3);
            }
            
            if (GUILayout.Button("Trigger in 5 Minutes"))
            {
                SetTriggerTime(5);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Trigger in 1 Hour"))
            {
                SetTriggerTime(60);
            }
            
            if (GUILayout.Button("Trigger in 1 Day"))
            {
                SetTriggerTime(60 * 24);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Time:", GUILayout.Width(85));
            customMinutes = EditorGUILayout.IntField(customMinutes, GUILayout.Width(60));
            EditorGUILayout.LabelField("minutes", GUILayout.Width(55));
            
            if (GUILayout.Button("Set Custom Time"))
            {
                if (customMinutes > 0)
                {
                    SetTriggerTime(customMinutes);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Time", "Please enter a positive number of minutes.", "OK");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (timeUntilTrigger.TotalSeconds < 0)
            {
                EditorGUILayout.HelpBox("⚠ This event has already triggered! Set a future time.", MessageType.Warning);
            }
            else if (timeUntilTrigger.TotalSeconds < 60)
            {
                EditorGUILayout.HelpBox($"⏰ Event triggers very soon! ({FormatTimeSpan(timeUntilTrigger)})", MessageType.Warning);
            }
        }
        
        private void SetTriggerTime(int minutesFromNow)
        {
            LiveEventData eventData = (LiveEventData)target;
            
            DateTime newTimeEst = LiveEventData.GetCurrentESTTime().AddMinutes(minutesFromNow);
            
            Undo.RecordObject(eventData, "Set Event Trigger Time");
            
            SerializedObject so = new SerializedObject(eventData);
            SerializedProperty timeProp = so.FindProperty("EventStartTimeEST");
            timeProp.stringValue = newTimeEst.ToString(TIME_FORMAT);
            so.ApplyModifiedProperties();
            
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(eventData);
            
            DateTime countdownStartTimeEst = newTimeEst.AddSeconds(-eventData.CountdownDuration);
            Debug.Log($"✅ Event '{eventData.EventName}' scheduled:\n" +
                      $"  Animation triggers: {newTimeEst.ToString(TIME_FORMAT)} EST ({minutesFromNow} min from now)\n" +
                      $"  Countdown starts: {countdownStartTimeEst.ToString(TIME_FORMAT)} EST (last {eventData.CountdownDuration}s)");
        }
        
        private string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalDays >= 1)
            {
                return $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m {span.Seconds}s";
            }
            else if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            }
            else if (span.TotalMinutes >= 1)
            {
                return $"{(int)span.TotalMinutes}m {span.Seconds}s";
            }
            else
            {
                return $"{(int)span.TotalSeconds}s";
            }
        }
    }
}
