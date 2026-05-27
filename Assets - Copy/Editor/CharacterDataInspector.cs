using UnityEngine;
using UnityEditor;

namespace TPSBR.UI.Editor
{
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataInspector : UnityEditor.Editor
    {
        private SerializedProperty _characterID;
        private SerializedProperty _displayName;
        private SerializedProperty _description;
        private SerializedProperty _icon;
        private SerializedProperty _price;
        private SerializedProperty _unlockedByDefault;
        private SerializedProperty _agentID;

        private void OnEnable()
        {
            _characterID = serializedObject.FindProperty("characterID");
            _displayName = serializedObject.FindProperty("displayName");
            _description = serializedObject.FindProperty("description");
            _icon = serializedObject.FindProperty("icon");
            _price = serializedObject.FindProperty("price");
            _unlockedByDefault = serializedObject.FindProperty("unlockedByDefault");
            _agentID = serializedObject.FindProperty("agentID");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CharacterData characterData = (CharacterData)target;

            EditorGUILayout.LabelField("Character Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawPreview(characterData);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_characterID);
            EditorGUILayout.PropertyField(_displayName);
            EditorGUILayout.PropertyField(_description);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shop Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_icon);
            EditorGUILayout.PropertyField(_price);
            EditorGUILayout.PropertyField(_unlockedByDefault, new GUIContent("Unlocked by Default"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Game Integration", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_agentID, new GUIContent("Agent ID (AgentSettings)"));

            if (string.IsNullOrEmpty(_agentID.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "Agent ID must match an entry in AgentSettings for the character to spawn correctly!",
                    MessageType.Warning
                );
            }

            if (_icon.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "No icon assigned! The character won't look good in the shop.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Validate Character Data", GUILayout.Height(30)))
            {
                characterData.Validate();
                EditorUtility.DisplayDialog("Validation", 
                    "Character data validation complete. Check the Console for any warnings.", 
                    "OK");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPreview(CharacterData characterData)
        {
            if (characterData.icon != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Shop Preview", EditorStyles.boldLabel);
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                Texture2D iconTexture = AssetPreview.GetAssetPreview(characterData.icon);
                if (iconTexture != null)
                {
                    GUILayout.Label(iconTexture, GUILayout.Width(100), GUILayout.Height(100));
                }
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Display Name:", characterData.displayName, EditorStyles.boldLabel);
                
                if (characterData.price > 0)
                {
                    EditorGUILayout.LabelField("Price:", $"{characterData.price} CloudCoins");
                }
                else
                {
                    EditorGUILayout.LabelField("Price:", "FREE", EditorStyles.boldLabel);
                }

                EditorGUILayout.LabelField("Status:", 
                    characterData.unlockedByDefault ? "Unlocked by Default" : "Must Purchase");
                
                EditorGUILayout.EndVertical();
            }
        }
    }
}
