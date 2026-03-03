using UnityEngine;
using UnityEditor;
using System.IO;

namespace TPSBR.UI.Editor
{
    public class CharacterDataEditor : EditorWindow
    {
        [MenuItem("TPSBR/Character & Shop Setup")]
        public static void ShowWindow()
        {
            GetWindow<CharacterDataEditor>("Character Setup");
        }

        private string _characterID = "new_character";
        private string _displayName = "New Character";
        private string _agentID = "";
        private Sprite _icon;
        private int _price = 500;
        private bool _unlockedByDefault = false;
        private Vector2 _scrollPosition;

        private ShopDatabase _shopDatabase;

        private void OnEnable()
        {
            LoadShopDatabase();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Character & Shop Setup Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawShopDatabaseSection();
            GUILayout.Space(20);
            DrawCreateCharacterSection();
            GUILayout.Space(20);
            DrawQuickSetupSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawShopDatabaseSection()
        {
            EditorGUILayout.HelpBox(
                "Shop Database stores all available characters. " +
                "Create one if you don't have it, or assign the existing one.",
                MessageType.Info
            );

            EditorGUILayout.BeginHorizontal();
            _shopDatabase = EditorGUILayout.ObjectField("Shop Database", _shopDatabase, typeof(ShopDatabase), false) as ShopDatabase;
            
            if (GUILayout.Button("Find Database", GUILayout.Width(120)))
            {
                LoadShopDatabase();
            }
            EditorGUILayout.EndHorizontal();

            if (_shopDatabase == null)
            {
                if (GUILayout.Button("Create New Shop Database", GUILayout.Height(30)))
                {
                    CreateShopDatabase();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Characters in Database:", _shopDatabase.characters.Count.ToString());
                
                if (GUILayout.Button("Open Shop Database", GUILayout.Height(25)))
                {
                    Selection.activeObject = _shopDatabase;
                    EditorGUIUtility.PingObject(_shopDatabase);
                }
            }
        }

        private void DrawCreateCharacterSection()
        {
            GUILayout.Label("Create New Character", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Create a new CharacterData asset with all the information needed for the shop.",
                MessageType.Info
            );

            _characterID = EditorGUILayout.TextField("Character ID", _characterID);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);
            _agentID = EditorGUILayout.TextField("Agent ID (AgentSettings)", _agentID);
            _icon = EditorGUILayout.ObjectField("Icon", _icon, typeof(Sprite), false) as Sprite;
            _price = EditorGUILayout.IntField("Price (CloudCoins)", _price);
            _unlockedByDefault = EditorGUILayout.Toggle("Unlocked by Default", _unlockedByDefault);

            GUILayout.Space(10);

            GUI.enabled = !string.IsNullOrEmpty(_characterID) && !string.IsNullOrEmpty(_displayName);
            
            if (GUILayout.Button("Create Character Data Asset", GUILayout.Height(35)))
            {
                CreateCharacterData();
            }
            
            GUI.enabled = true;
        }

        private void DrawQuickSetupSection()
        {
            GUILayout.Label("Quick Setup Actions", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Quick actions to help you set up the shop system.",
                MessageType.Info
            );

            if (GUILayout.Button("Validate Shop Database", GUILayout.Height(30)))
            {
                if (_shopDatabase != null)
                {
                    _shopDatabase.ValidateDatabase();
                    EditorUtility.DisplayDialog("Validation Complete", 
                        "Check the Console for any warnings or errors.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", 
                        "Please assign or create a Shop Database first.", "OK");
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Open Materials Folder", GUILayout.Height(25)))
            {
                OpenFolder("Assets/Materials");
            }

            if (GUILayout.Button("Open Prefabs Folder", GUILayout.Height(25)))
            {
                OpenFolder("Assets/Prefabs");
            }
        }

        private void LoadShopDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:ShopDatabase");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _shopDatabase = AssetDatabase.LoadAssetAtPath<ShopDatabase>(path);
                
                if (guids.Length > 1)
                {
                    Debug.LogWarning($"Found {guids.Length} ShopDatabase assets. Using the first one found.");
                }
            }
        }

        private void CreateShopDatabase()
        {
            string path = "Assets/Scripts/ShopDatabase.asset";
            
            if (!Directory.Exists("Assets/Scripts"))
            {
                Directory.CreateDirectory("Assets/Scripts");
            }

            ShopDatabase database = CreateInstance<ShopDatabase>();
            database.startingCloudCoins = 100;

            AssetDatabase.CreateAsset(database, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _shopDatabase = database;
            
            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
            
            EditorUtility.DisplayDialog("Success", 
                $"Created Shop Database at {path}", "OK");
        }

        private void CreateCharacterData()
        {
            string directoryPath = "Assets/Scripts/CharacterData";
            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string sanitizedID = _characterID.Replace(" ", "_").ToLower();
            string path = $"{directoryPath}/{sanitizedID}.asset";

            if (File.Exists(path))
            {
                if (!EditorUtility.DisplayDialog("File Exists", 
                    $"A character with ID '{_characterID}' already exists. Overwrite?", 
                    "Yes", "No"))
                {
                    return;
                }
            }

            CharacterData characterData = CreateInstance<CharacterData>();
            characterData.characterID = _characterID;
            characterData.displayName = _displayName;
            characterData.agentID = _agentID;
            characterData.icon = _icon;
            characterData.price = _price;
            characterData.unlockedByDefault = _unlockedByDefault;

            AssetDatabase.CreateAsset(characterData, path);
            AssetDatabase.SaveAssets();

            if (_shopDatabase != null)
            {
                if (!_shopDatabase.characters.Contains(characterData))
                {
                    _shopDatabase.characters.Add(characterData);
                    EditorUtility.SetDirty(_shopDatabase);
                    AssetDatabase.SaveAssets();
                }
            }

            AssetDatabase.Refresh();
            
            Selection.activeObject = characterData;
            EditorGUIUtility.PingObject(characterData);
            
            EditorUtility.DisplayDialog("Success", 
                $"Created CharacterData at {path}" + 
                (_shopDatabase != null ? "\n\nAdded to Shop Database!" : "\n\nRemember to add it to Shop Database manually."), 
                "OK");

            _characterID = "new_character";
            _displayName = "New Character";
            _agentID = "";
            _icon = null;
            _price = 500;
            _unlockedByDefault = false;
        }

        private void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
