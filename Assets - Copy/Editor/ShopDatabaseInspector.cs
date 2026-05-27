using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace TPSBR.UI.Editor
{
    [CustomEditor(typeof(ShopDatabase))]
    public class ShopDatabaseInspector : UnityEditor.Editor
    {
        private SerializedProperty _characters;
        private SerializedProperty _startingCloudCoins;
        private ReorderableList _characterList;

        private void OnEnable()
        {
            _characters = serializedObject.FindProperty("characters");
            _startingCloudCoins = serializedObject.FindProperty("startingCloudCoins");

            _characterList = new ReorderableList(serializedObject, _characters, true, true, true, true);
            
            _characterList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Shop Characters");
            };

            _characterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = _characterList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                CharacterData characterData = element.objectReferenceValue as CharacterData;
                
                if (characterData != null)
                {
                    float iconSize = EditorGUIUtility.singleLineHeight * 2;
                    Rect iconRect = new Rect(rect.x, rect.y, iconSize, iconSize);
                    Rect nameRect = new Rect(rect.x + iconSize + 5, rect.y, rect.width - iconSize - 150, EditorGUIUtility.singleLineHeight);
                    Rect priceRect = new Rect(rect.x + iconSize + 5, rect.y + EditorGUIUtility.singleLineHeight, 150, EditorGUIUtility.singleLineHeight);
                    Rect fieldRect = new Rect(rect.x + rect.width - 140, rect.y, 140, EditorGUIUtility.singleLineHeight * 2);

                    if (characterData.icon != null)
                    {
                        Texture2D iconTexture = AssetPreview.GetAssetPreview(characterData.icon);
                        if (iconTexture != null)
                        {
                            GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit);
                        }
                    }

                    EditorGUI.LabelField(nameRect, characterData.displayName, EditorStyles.boldLabel);
                    
                    string priceText = characterData.price > 0 ? 
                        $"💰 {characterData.price} CloudCoins" : 
                        "FREE";
                    
                    if (characterData.unlockedByDefault)
                    {
                        priceText += " (Default)";
                    }
                    
                    EditorGUI.LabelField(priceRect, priceText);

                    EditorGUI.PropertyField(fieldRect, element, GUIContent.none);
                }
                else
                {
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                }
            };

            _characterList.elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShopDatabase database = (ShopDatabase)target;

            EditorGUILayout.LabelField("Shop Database Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawSummary(database);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_startingCloudCoins);

            EditorGUILayout.Space(10);

            _characterList.DoLayoutList();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate Database", GUILayout.Height(30)))
            {
                database.ValidateDatabase();
                EditorUtility.DisplayDialog("Validation Complete", 
                    "Database validation complete. Check the Console for any warnings or errors.", 
                    "OK");
            }

            if (GUILayout.Button("Open Character Setup Tool", GUILayout.Height(30)))
            {
                CharacterDataEditor.ShowWindow();
            }
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary(ShopDatabase database)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Database Summary", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Total Characters:", database.characters.Count.ToString());
            
            var defaultUnlocked = database.GetDefaultUnlockedCharacters();
            EditorGUILayout.LabelField("Default Unlocked:", defaultUnlocked.Count.ToString());

            int totalCost = 0;
            foreach (var character in database.characters)
            {
                if (character != null && !character.unlockedByDefault)
                {
                    totalCost += character.price;
                }
            }
            
            EditorGUILayout.LabelField("Total Cost (All Locked):", $"{totalCost} CloudCoins");

            EditorGUILayout.EndVertical();

            if (defaultUnlocked.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ No characters are set as 'Unlocked by Default'! " +
                    "Players will start with no characters. Add at least one default character.",
                    MessageType.Warning
                );
            }
        }
    }
}
