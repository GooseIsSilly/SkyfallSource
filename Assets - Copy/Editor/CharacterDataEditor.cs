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
        private SkinRarity _rarity = SkinRarity.Common;
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
            _rarity = (SkinRarity)EditorGUILayout.EnumPopup("Rarity", _rarity);
            _price = EditorGUILayout.IntField("Price (CloudCoins)", _price);
            _unlockedByDefault = EditorGUILayout.Toggle("Unlocked by Default", _unlockedByDefault);

            // Live rarity colour preview
            GUILayout.Space(4);
            Color rarityColor = GetEditorRarityColor(_rarity);
            var prevColor = GUI.color;
            GUI.color = rarityColor;
            EditorGUILayout.LabelField($"▶  {_rarity.ToString().ToUpper()}", EditorStyles.boldLabel);
            GUI.color = prevColor;

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

            GUILayout.Space(10);
            GUILayout.Label("Edit Existing Character Rarities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select any CharacterData asset in the Project window and edit it directly — " +
                "the Rarity and Price fields appear in its Inspector. " +
                "Or use the bulk tool below to set all rarities at once.",
                MessageType.Info
            );

            if (GUILayout.Button("Bulk-Edit All Character Rarities…", GUILayout.Height(30)))
            {
                BulkRarityEditorWindow.ShowWindow(_shopDatabase);
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
            characterData.characterID      = _characterID;
            characterData.displayName      = _displayName;
            characterData.agentID          = _agentID;
            characterData.icon             = _icon;
            characterData.rarity           = _rarity;
            characterData.price            = _price;
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

            _characterID       = "new_character";
            _displayName       = "New Character";
            _agentID           = "";
            _icon              = null;
            _rarity            = SkinRarity.Common;
            _price             = 500;
            _unlockedByDefault = false;
        }

        /// <summary>Returns a visible editor-friendly colour for the given rarity.</summary>
        private static Color GetEditorRarityColor(SkinRarity rarity)
        {
            return rarity switch
            {
                SkinRarity.Common    => new Color(0.75f, 0.75f, 0.75f),
                SkinRarity.Rare      => new Color(0.3f,  0.6f,  1f),
                SkinRarity.Epic      => new Color(0.7f,  0.3f,  1f),
                SkinRarity.Legendary => new Color(1f,    0.65f, 0.1f),
                SkinRarity.Mythic    => new Color(1f,    0.3f,  0.35f),
                _                   => Color.white
            };
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

    // =========================================================================
    // Bulk Rarity Editor — opened via the Character & Shop Setup window
    // =========================================================================

    public class BulkRarityEditorWindow : EditorWindow
    {
        private ShopDatabase _database;
        private Vector2 _scroll;

        // Parallel arrays tracking per-row edits (mirrors _database.characters order)
        private SkinRarity[] _pendingRarities;
        private int[]        _pendingPrices;
        private bool         _dirty;

        public static void ShowWindow(ShopDatabase database)
        {
            var window = GetWindow<BulkRarityEditorWindow>("Bulk Rarity Editor");
            window.LoadDatabase(database);
            window.minSize = new Vector2(500, 300);
        }

        private void LoadDatabase(ShopDatabase database)
        {
            _database = database;
            RebuildArrays();
        }

        private void RebuildArrays()
        {
            if (_database == null) return;

            int count = _database.characters.Count;
            _pendingRarities = new SkinRarity[count];
            _pendingPrices   = new int[count];
            _dirty = false;

            for (int i = 0; i < count; i++)
            {
                var c = _database.characters[i];
                _pendingRarities[i] = c != null ? c.rarity : SkinRarity.Common;
                _pendingPrices[i]   = c != null ? c.price  : 0;
            }
        }

        private void OnGUI()
        {
            if (_database == null)
            {
                EditorGUILayout.HelpBox("No ShopDatabase loaded. Open via TPSBR → Character & Shop Setup.", MessageType.Warning);
                return;
            }

            GUILayout.Label($"Editing {_database.characters.Count} characters in {_database.name}", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Changes are staged here. Press Apply to save to disk.", MessageType.Info);
            GUILayout.Space(6);

            // Header row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Character",  EditorStyles.boldLabel, GUILayout.Width(160));
            EditorGUILayout.LabelField("Rarity",     EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.LabelField("Price",      EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Preview",    EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _database.characters.Count; i++)
            {
                var character = _database.characters[i];
                if (character == null) continue;

                EditorGUILayout.BeginHorizontal();

                // Name
                EditorGUILayout.LabelField(
                    string.IsNullOrEmpty(character.displayName) ? character.characterID : character.displayName,
                    GUILayout.Width(160));

                // Rarity dropdown
                var newRarity = (SkinRarity)EditorGUILayout.EnumPopup(_pendingRarities[i], GUILayout.Width(120));
                if (newRarity != _pendingRarities[i]) { _pendingRarities[i] = newRarity; _dirty = true; }

                // Price field
                var newPrice = EditorGUILayout.IntField(_pendingPrices[i], GUILayout.Width(80));
                if (newPrice != _pendingPrices[i]) { _pendingPrices[i] = newPrice; _dirty = true; }

                // Colour swatch
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = RarityToColor(_pendingRarities[i]);
                GUILayout.Box(_pendingRarities[i].ToString().ToUpper(), GUILayout.Width(100));
                GUI.backgroundColor = prevBg;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _dirty;
            if (GUILayout.Button("Apply All Changes", GUILayout.Height(32)))
                ApplyChanges();
            GUI.enabled = true;

            if (GUILayout.Button("Revert", GUILayout.Width(80), GUILayout.Height(32)))
                RebuildArrays();

            EditorGUILayout.EndHorizontal();

            if (_dirty)
                EditorGUILayout.HelpBox("You have unsaved changes.", MessageType.Warning);
        }

        private void ApplyChanges()
        {
            for (int i = 0; i < _database.characters.Count; i++)
            {
                var character = _database.characters[i];
                if (character == null) continue;

                character.rarity = _pendingRarities[i];
                character.price  = _pendingPrices[i];
                EditorUtility.SetDirty(character);
            }

            AssetDatabase.SaveAssets();
            _dirty = false;
            Debug.Log($"[BulkRarityEditor] Saved rarity/price changes for {_database.characters.Count} characters.");
        }

        private static Color RarityToColor(SkinRarity r) => r switch
        {
            SkinRarity.Common    => new Color(0.75f, 0.75f, 0.75f),
            SkinRarity.Rare      => new Color(0.3f,  0.6f,  1f),
            SkinRarity.Epic      => new Color(0.7f,  0.3f,  1f),
            SkinRarity.Legendary => new Color(1f,    0.65f, 0.1f),
            SkinRarity.Mythic    => new Color(1f,    0.3f,  0.35f),
            _                   => Color.white
        };
    }
}
