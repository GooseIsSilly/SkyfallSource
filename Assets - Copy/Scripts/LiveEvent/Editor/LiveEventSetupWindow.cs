using UnityEngine;
using UnityEditor;

namespace TPSBR
{
    public class LiveEventSetupWindow : EditorWindow
    {
        private LiveEventData _eventData;
        private GameObject _countdownPrefab;
        private bool _setupComplete;
        
        [MenuItem("Window/Skyfall/Live Event Setup Wizard")]
        private static void ShowWindow()
        {
            LiveEventSetupWindow window = GetWindow<LiveEventSetupWindow>("Live Event Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Live Event System Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Follow these steps to create a Fortnite-style live event!", MessageType.Info);
            
            GUILayout.Space(10);
            
            DrawStep1();
            DrawStep2();
            DrawStep3();
            DrawStep4();
            
            GUILayout.Space(20);
            
            if (_setupComplete)
            {
                EditorGUILayout.HelpBox("✓ Setup Complete! Press Play to see your event countdown!", MessageType.Info);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Documentation", GUILayout.Height(30)))
            {
                Application.OpenURL("https://docs.unity3d.com");
            }
        }
        
        private void DrawStep1()
        {
            EditorGUILayout.LabelField("Step 1: Create Event Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Event Data:", GUILayout.Width(100));
            _eventData = (LiveEventData)EditorGUILayout.ObjectField(_eventData, typeof(LiveEventData), false);
            EditorGUILayout.EndHorizontal();
            
            if (_eventData == null)
            {
                if (GUILayout.Button("Create New Event Data"))
                {
                    LiveEventData newData = CreateInstance<LiveEventData>();
                    newData.EventName = "My First Event";
                    
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Save Event Data", 
                        "MyLiveEvent", 
                        "asset",
                        "Create a new Live Event Data asset"
                    );
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(newData, path);
                        AssetDatabase.SaveAssets();
                        _eventData = newData;
                        EditorGUIUtility.PingObject(newData);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Event Data created!", MessageType.None);
                
                if (GUILayout.Button("Set Event Time to 1 Minute from Now (EST)"))
                {
                    SerializedObject so = new SerializedObject(_eventData);
                    so.FindProperty("EventStartTimeEST").stringValue = 
                        LiveEventData.GetCurrentESTTime().AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_eventData);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Event time set to 1 minute from now (EST)!");
                }
            }
            
            GUILayout.Space(10);
        }
        
        private void DrawStep2()
        {
            EditorGUILayout.LabelField("Step 2: Create Countdown Display", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create Countdown Prefab"))
            {
                LiveEventSetupHelper.CreateWorldCountdownText();
            }
            
            EditorGUILayout.HelpBox("Or drag existing prefab from Project window into scene", MessageType.Info);
            
            GUILayout.Space(10);
        }
        
        private void DrawStep3()
        {
            EditorGUILayout.LabelField("Step 3: Setup Your Satellite", EditorStyles.boldLabel);
            
            GameObject selected = Selection.activeGameObject;
            
            if (selected != null)
            {
                EditorGUILayout.HelpBox($"Selected: {selected.name}", MessageType.None);
                
                if (GUILayout.Button($"Setup '{selected.name}' as Event Object"))
                {
                    Animation anim = selected.GetComponent<Animation>();
                    if (anim == null)
                    {
                        anim = Undo.AddComponent<Animation>(selected);
                    }
                    
                    LiveEventAnimationTrigger trigger = selected.GetComponent<LiveEventAnimationTrigger>();
                    if (trigger == null)
                    {
                        trigger = Undo.AddComponent<LiveEventAnimationTrigger>(selected);
                    }
                    
                    if (_eventData != null)
                    {
                        SerializedObject so = new SerializedObject(trigger);
                        so.FindProperty("_targetEvent").objectReferenceValue = _eventData;
                        so.ApplyModifiedProperties();
                    }
                    
                    Debug.Log($"Setup complete for {selected.name}!");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select your satellite GameObject in the scene first", MessageType.Warning);
            }
            
            GUILayout.Space(10);
        }
        
        private void DrawStep4()
        {
            EditorGUILayout.LabelField("Step 4: Add Event Manager", EditorStyles.boldLabel);
            
            LiveEventManager existingManager = FindFirstObjectByType<LiveEventManager>();
            
            if (existingManager != null)
            {
                EditorGUILayout.HelpBox("✓ LiveEventManager found in scene!", MessageType.None);
                
                if (_eventData != null && GUILayout.Button("Assign Event Data to Manager"))
                {
                    SerializedObject so = new SerializedObject(existingManager);
                    SerializedProperty arrayProp = so.FindProperty("_liveEvents");
                    
                    bool alreadyAdded = false;
                    for (int i = 0; i < arrayProp.arraySize; i++)
                    {
                        if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == _eventData)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }
                    
                    if (!alreadyAdded)
                    {
                        arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
                        arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1).objectReferenceValue = _eventData;
                        so.ApplyModifiedProperties();
                        Debug.Log("Event data added to manager!");
                    }
                    
                    _setupComplete = true;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No LiveEventManager in scene", MessageType.Warning);
                
                if (GUILayout.Button("Create LiveEventManager"))
                {
                    GameObject managerObj = new GameObject("LiveEventManager");
                    LiveEventManager manager = managerObj.AddComponent<LiveEventManager>();
                    
                    if (_eventData != null)
                    {
                        SerializedObject so = new SerializedObject(manager);
                        SerializedProperty arrayProp = so.FindProperty("_liveEvents");
                        arrayProp.InsertArrayElementAtIndex(0);
                        arrayProp.GetArrayElementAtIndex(0).objectReferenceValue = _eventData;
                        so.ApplyModifiedProperties();
                    }
                    
                    Selection.activeGameObject = managerObj;
                    EditorGUIUtility.PingObject(managerObj);
                    
                    Debug.Log("LiveEventManager created! Make sure it's spawned as a networked object.");
                }
            }
            
            EditorGUILayout.HelpBox("⚠ Important: LiveEventManager must be spawned by Fusion networking!", MessageType.Warning);
        }
    }
}
