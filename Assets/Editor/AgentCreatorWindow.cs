using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TPSBR.Tools.Editor
{
	/// <summary>
	/// One-click tool that turns a Humanoid character model into a fully wired TPSBR Agent prefab.
	/// Creates a Prefab Variant of AgentBase, nests the given model under VisualsRoot, wires up
	/// Character/CharacterAnimationController/Weapons/HitboxRoot references, creates the weapon and
	/// grenade/belt/back attachment handles, adds BodyPart hitboxes, and assigns the shared Humanoid
	/// Avatar used by the existing Sci-Fi Character rig family so all existing animations apply as-is.
	/// Requires the model to use the same bone naming convention as the existing agents
	/// (root/pelvis/spine_01-03/clavicle_l-r/upperarm/lowerarm/hand_l-r/middle_01_r/neck_01/head/thigh/calf/foot_l-r).
	/// </summary>
	public class AgentCreatorWindow : EditorWindow
	{
		private const string AgentBasePrefabPath = "Assets/TPSBR/Prefabs/Agents/AgentBase.prefab";
		private const string DefaultOutputFolder = "Assets/TPSBR/Prefabs/Agents";
		private const string SharedAvatarModelPath = "Assets/3rdParty/Maksim Bugrimov/Sci_Fi_Character_08/Mesh/Sci_Fi_Character_08_Generic.FBX";
		private const string SharedAvatarSubAssetName = "Sci_Fi_Character_08_GenericAvatar";

		private struct BoneTransform
		{
			public string ParentBone;
			public Vector3 LocalPosition;
			public Vector3 LocalEulerAngles;

			public BoneTransform(string parentBone, Vector3 localPosition, Vector3 localEulerAngles)
			{
				ParentBone = parentBone;
				LocalPosition = localPosition;
				LocalEulerAngles = localEulerAngles;
			}
		}

		private enum HitboxType
		{
			Box,
			Sphere,
		}

		private struct HitboxConfig
		{
			public HitboxType Type;
			public float SphereRadius;
			public Vector3 BoxExtents;
			public Vector3 Offset;
			public float DamageMultiplier;
			public bool IsCritical;

			public HitboxConfig(HitboxType type, float sphereRadius, Vector3 boxExtents, Vector3 offset, float damageMultiplier, bool isCritical)
			{
				Type = type;
				SphereRadius = sphereRadius;
				BoxExtents = boxExtents;
				Offset = offset;
				DamageMultiplier = damageMultiplier;
				IsCritical = isCritical;
			}
		}

		// Default attachment handle placements, sourced from the existing Soldier/Marine agents.
		// Users should nudge these afterwards to align weapon grips precisely for a new mesh.
		private static readonly Dictionary<string, BoneTransform> HandleDefaults = new Dictionary<string, BoneTransform>
		{
			{ "WeaponHandlePistol", new BoneTransform("middle_01_r", new Vector3(0.0152153f, -0.0656177f, 0.0212519f), new Vector3(300.258972f, 157.629929f, 12.9928837f)) },
			{ "WeaponHandleRifle",  new BoneTransform("middle_01_r", new Vector3(0.0102423f, -0.0605143f, 0.0368993f), new Vector3(316.153656f, 112.114662f, 44.16746f)) },
			{ "BackHandle",         new BoneTransform("spine_03",    new Vector3(-0.1069214f, 0.1862862f, -0.0419999f), new Vector3(352.730072f, 88.39002f, 90f)) },
			{ "BeltHandle",         new BoneTransform("spine_01",    new Vector3(0f, 0f, 0.185f), new Vector3(0f, 78.8063354f, 180f)) },
			{ "GrenadeHandle1",     new BoneTransform("spine_01",    new Vector3(-0.0353924f, -0.116944f, 0.101f), new Vector3(0f, 78.8063354f, 180f)) },
			{ "GrenadeHandle2",     new BoneTransform("spine_01",    new Vector3(-0.0183923f, -0.116944f, 0.016f), new Vector3(0f, 78.8063354f, 180f)) },
			{ "GrenadeHandle3",     new BoneTransform("spine_01",    new Vector3(0.0016076f, -0.116944f, -0.087f), new Vector3(0f, 78.8063354f, 180f)) },
		};

		// Default hitbox shapes per bone, sourced from the existing Marine agent.
		private static readonly Dictionary<string, HitboxConfig> HitboxDefaults = new Dictionary<string, HitboxConfig>
		{
			{ "spine_02",    new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.3f, 0.17f, 0.22f), new Vector3(0f, 0f, 0f), 1f, false) },
			{ "upperarm_l",  new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.15f, 0.08f, 0.08f), new Vector3(-0.15f, 0f, 0f), 1f, false) },
			{ "lowerarm_l",  new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.15f, 0.06f, 0.06f), new Vector3(-0.16f, 0f, 0f), 1f, false) },
			{ "upperarm_r",  new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.15f, 0.08f, 0.08f), new Vector3(0.15f, 0f, 0f), 1f, false) },
			{ "lowerarm_r",  new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.15f, 0.06f, 0.06f), new Vector3(0.16f, 0f, 0f), 1f, false) },
			{ "head",        new HitboxConfig(HitboxType.Sphere, 0.17f, Vector3.zero, new Vector3(-0.03f, 0f, 0f), 3f, true) },
			{ "thigh_l",     new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.2f, 0.12f, 0.12f), new Vector3(0.18f, 0f, 0f), 0.6f, false) },
			{ "calf_l",      new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.3f, 0.08f, 0.08f), new Vector3(0.2f, 0f, 0f), 0.4f, false) },
			{ "thigh_r",     new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.2f, 0.12f, 0.12f), new Vector3(-0.17f, 0f, 0f), 0.6f, false) },
			{ "calf_r",      new HitboxConfig(HitboxType.Box, 0f, new Vector3(0.3f, 0.08f, 0.08f), new Vector3(-0.2f, 0f, 0f), 0.4f, false) },
		};

		[MenuItem("TPSBR/Agent Creator")]
		public static void ShowWindow()
		{
			GetWindow<AgentCreatorWindow>("Agent Creator");
		}

		private GameObject _characterModel;
		private string _agentName = "NewAgent";
		private bool _forceHumanoidImport = true;
		private Avatar _avatarOverride;
		private string _outputFolder = DefaultOutputFolder;

		private void OnGUI()
		{
			GUILayout.Label("Create New Agent From Model", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox(
				"Drag in a Humanoid-rigged character model (FBX) that uses the same bone naming " +
				"convention as the existing agents (root, pelvis, spine_01-03, clavicle_l/r, " +
				"upperarm/lowerarm/hand_l/r, middle_01_r, neck_01/head, thigh/calf/foot_l/r). " +
				"This creates a Prefab Variant of AgentBase with the model nested in VisualsRoot, " +
				"the shared Humanoid Avatar assigned, weapon/grenade/belt/back handles created, and " +
				"hitboxes wired up so all existing animations and gameplay scripts apply immediately.",
				MessageType.Info);

			GUILayout.Space(8);

			_characterModel = (GameObject)EditorGUILayout.ObjectField("Character Model", _characterModel, typeof(GameObject), false);
			_agentName = EditorGUILayout.TextField("New Agent Name", _agentName);
			_outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);
			_forceHumanoidImport = EditorGUILayout.Toggle("Force Humanoid Import", _forceHumanoidImport);
			_avatarOverride = (Avatar)EditorGUILayout.ObjectField("Avatar Override (optional)", _avatarOverride, typeof(Avatar), false);

			GUILayout.Space(12);

			GUI.enabled = _characterModel != null && !string.IsNullOrWhiteSpace(_agentName);
			if (GUILayout.Button("Create Agent Prefab", GUILayout.Height(36)))
			{
				CreateAgent();
			}
			GUI.enabled = true;
		}

		private void CreateAgent()
		{
			string modelPath = AssetDatabase.GetAssetPath(_characterModel);
			if (string.IsNullOrEmpty(modelPath))
			{
				EditorUtility.DisplayDialog("Agent Creator", "Selected Character Model is not a saved asset.", "OK");
				return;
			}

			if (_forceHumanoidImport && EnsureHumanoidImport(modelPath) == false)
			{
				EditorUtility.DisplayDialog("Agent Creator", "Failed to force Humanoid import on the model. Check the Console for details.", "OK");
				return;
			}

			GameObject agentBaseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AgentBasePrefabPath);
			if (agentBaseAsset == null)
			{
				EditorUtility.DisplayDialog("Agent Creator", $"Could not find AgentBase prefab at {AgentBasePrefabPath}.", "OK");
				return;
			}

			if (!System.IO.Directory.Exists(_outputFolder))
			{
				System.IO.Directory.CreateDirectory(_outputFolder);
			}

			string sanitizedName = _agentName.Replace(" ", string.Empty);
			string newPrefabPath = $"{_outputFolder}/{sanitizedName}.prefab";

			var previewScene = EditorSceneManager.NewPreviewScene();
			try
			{
				GameObject agentRoot = (GameObject)PrefabUtility.InstantiatePrefab(agentBaseAsset, previewScene);
				agentRoot.name = sanitizedName;

				Transform visualsRoot = agentRoot.transform.Find("VisualsRoot");
				if (visualsRoot == null)
				{
					EditorUtility.DisplayDialog("Agent Creator", "AgentBase prefab has no VisualsRoot child. Aborting.", "OK");
					return;
				}

				GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(_characterModel, previewScene);
				modelInstance.transform.SetParent(visualsRoot, false);

				SetupAnimator(modelInstance);

				Transform modelRoot = FindBone(modelInstance.transform, "root") ?? modelInstance.transform;
				var boneMap = BuildBoneMap(modelRoot);

				WireCharacter(agentRoot, boneMap);
				WireAnimationController(agentRoot, modelInstance);
				WireWeapons(agentRoot, boneMap);
				WireHitboxes(agentRoot, boneMap);

				PrefabUtility.SaveAsPrefabAsset(agentRoot, newPrefabPath);
				Debug.Log($"[AgentCreator] Created agent prefab at {newPrefabPath}. Review weapon handle placement, register the prefab's NetworkObject with Fusion's Network Project Config, and add an AgentSetup entry in your AgentSettings asset.");
			}
			finally
			{
				EditorSceneManager.ClosePreviewScene(previewScene);
			}

			AssetDatabase.Refresh();
			var created = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
			Selection.activeObject = created;
			EditorGUIUtility.PingObject(created);
		}

		private bool EnsureHumanoidImport(string modelPath)
		{
			var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
			if (importer == null)
			{
				return false;
			}

			if (importer.animationType != ModelImporterAnimationType.Human)
			{
				importer.animationType = ModelImporterAnimationType.Human;
				importer.SaveAndReimport();
			}

			return true;
		}

		private void SetupAnimator(GameObject modelInstance)
		{
			Animator animator = modelInstance.GetComponent<Animator>();
			if (animator == null)
			{
				animator = modelInstance.AddComponent<Animator>();
			}

			Avatar avatar = _avatarOverride;
			if (avatar == null)
			{
				avatar = AssetDatabase.LoadAllAssetsAtPath(SharedAvatarModelPath)
					.OfType<Avatar>()
					.FirstOrDefault(a => a.name == SharedAvatarSubAssetName);
			}

			if (avatar == null)
			{
				string modelPath = AssetDatabase.GetAssetPath(modelInstance);
				avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
				if (avatar != null)
				{
					Debug.LogWarning("[AgentCreator] Shared Sci-Fi Character avatar not found, falling back to the model's own generated Avatar. Animation retargeting may need manual review.");
				}
			}

			animator.avatar = avatar;
			animator.runtimeAnimatorController = null;
			animator.applyRootMotion = false;
			animator.updateMode = AnimatorUpdateMode.Normal;
			animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
		}

		private Transform FindBone(Transform root, string boneName)
		{
			if (root.name == boneName)
			{
				return root;
			}

			foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
			{
				if (child.name == boneName)
				{
					return child;
				}
			}

			return null;
		}

		private Dictionary<string, Transform> BuildBoneMap(Transform modelRoot)
		{
			var map = new Dictionary<string, Transform>();
			foreach (Transform bone in modelRoot.GetComponentsInChildren<Transform>(true))
			{
				if (!map.ContainsKey(bone.name))
				{
					map[bone.name] = bone;
				}
			}

			foreach (var handle in HandleDefaults)
			{
				if (map.ContainsKey(handle.Key))
				{
					continue;
				}

				if (!map.TryGetValue(handle.Value.ParentBone, out Transform parentBone))
				{
					Debug.LogWarning($"[AgentCreator] Could not find bone '{handle.Value.ParentBone}' to attach handle '{handle.Key}'. Skipping.");
					continue;
				}

				var handleTransform = new GameObject(handle.Key).transform;
				handleTransform.SetParent(parentBone, false);
				handleTransform.localPosition = handle.Value.LocalPosition;
				handleTransform.localEulerAngles = handle.Value.LocalEulerAngles;
				handleTransform.localScale = Vector3.one;
				handleTransform.gameObject.tag = "Player";
				handleTransform.gameObject.layer = parentBone.gameObject.layer;

				map[handle.Key] = handleTransform;
			}

			return map;
		}

		private void WireCharacter(GameObject agentRoot, Dictionary<string, Transform> boneMap)
		{
			Character character = agentRoot.GetComponent<Character>();
			if (character == null)
			{
				return;
			}

			var so = new SerializedObject(character);
			var view = so.FindProperty("_thirdPersonView");

			AssignTransform(view.FindPropertyRelative("RootBone"), boneMap, "root");
			AssignTransform(view.FindPropertyRelative("HeadTransform"), boneMap, "head");
			AssignTransform(view.FindPropertyRelative("LeftFoot"), boneMap, "foot_l");
			AssignTransform(view.FindPropertyRelative("RightFoot"), boneMap, "foot_r");

			so.ApplyModifiedPropertiesWithoutUndo();
		}

		private void WireAnimationController(GameObject agentRoot, GameObject modelInstance)
		{
			Component controller = agentRoot.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().Name == "CharacterAnimationController");
			if (controller == null)
			{
				return;
			}

			var so = new SerializedObject(controller);

			var leftHand = FindBone(modelInstance.transform, "hand_l");
			var leftLowerArm = FindBone(modelInstance.transform, "lowerarm_l");
			var leftUpperArm = FindBone(modelInstance.transform, "upperarm_l");

			SetObjectReference(so.FindProperty("_leftHand"), leftHand);
			SetObjectReference(so.FindProperty("_leftLowerArm"), leftLowerArm);
			SetObjectReference(so.FindProperty("_leftUpperArm"), leftUpperArm);
			SetObjectReference(so.FindProperty("_animator"), modelInstance.GetComponent<Animator>());

			so.ApplyModifiedPropertiesWithoutUndo();
		}

		private void WireWeapons(GameObject agentRoot, Dictionary<string, Transform> boneMap)
		{
			Weapons weapons = agentRoot.GetComponent<Weapons>();
			if (weapons == null)
			{
				return;
			}

			var so = new SerializedObject(weapons);
			var slots = so.FindProperty("_slots");

			var slotDefs = new[]
			{
				("WeaponHandlePistol", "BackHandle"),
				("WeaponHandlePistol", "BeltHandle"),
				("WeaponHandleRifle",  "BackHandle"),
				(null as string, null as string),
				(null as string, null as string),
				("WeaponHandlePistol", "GrenadeHandle1"),
				("WeaponHandlePistol", "GrenadeHandle2"),
				("WeaponHandlePistol", "GrenadeHandle3"),
			};

			slots.arraySize = slotDefs.Length;
			for (int i = 0; i < slotDefs.Length; i++)
			{
				var element = slots.GetArrayElementAtIndex(i);
				var (activeName, inactiveName) = slotDefs[i];

				AssignTransform(element.FindPropertyRelative("Active"), boneMap, activeName);
				AssignTransform(element.FindPropertyRelative("Inactive"), boneMap, inactiveName);
			}

			so.ApplyModifiedPropertiesWithoutUndo();
		}

		private void WireHitboxes(GameObject agentRoot, Dictionary<string, Transform> boneMap)
		{
			Fusion.HitboxRoot hitboxRoot = agentRoot.GetComponent<Fusion.HitboxRoot>();
			if (hitboxRoot == null)
			{
				return;
			}

			var hitboxComponents = new List<Hitbox>();
			foreach (var kvp in HitboxDefaults)
			{
				if (!boneMap.TryGetValue(kvp.Key, out Transform bone))
				{
					Debug.LogWarning($"[AgentCreator] Could not find bone '{kvp.Key}' for a hitbox. Skipping.");
					continue;
				}

				BodyPart bodyPart = bone.GetComponent<BodyPart>();
				if (bodyPart == null)
				{
					bodyPart = bone.gameObject.AddComponent<BodyPart>();
				}

				var bpSo = new SerializedObject(bodyPart);
				bpSo.FindProperty("_damageMultiplier").floatValue = kvp.Value.DamageMultiplier;
				bpSo.FindProperty("_isCritical").boolValue = kvp.Value.IsCritical;

				var typeProp = bpSo.FindProperty("Type");
				if (typeProp != null)
				{
					typeProp.enumValueIndex = (int)kvp.Value.Type;
				}

				SetIfExists(bpSo, "SphereRadius", kvp.Value.SphereRadius);
				SetIfExists(bpSo, "BoxExtents", kvp.Value.BoxExtents);
				SetIfExists(bpSo, "Offset", kvp.Value.Offset);

				bpSo.ApplyModifiedPropertiesWithoutUndo();

				hitboxComponents.Add(bodyPart);
			}

			var hbSo = new SerializedObject(hitboxRoot);
			var hitboxesProp = hbSo.FindProperty("Hitboxes");
			hitboxesProp.arraySize = hitboxComponents.Count;
			for (int i = 0; i < hitboxComponents.Count; i++)
			{
				hitboxesProp.GetArrayElementAtIndex(i).objectReferenceValue = hitboxComponents[i];
			}

			hbSo.ApplyModifiedPropertiesWithoutUndo();
		}

		private void SetIfExists(SerializedObject so, string propertyName, float value)
		{
			var prop = so.FindProperty(propertyName);
			if (prop != null)
			{
				prop.floatValue = value;
			}
		}

		private void SetIfExists(SerializedObject so, string propertyName, Vector3 value)
		{
			var prop = so.FindProperty(propertyName);
			if (prop != null)
			{
				prop.vector3Value = value;
			}
		}

		private void AssignTransform(SerializedProperty property, Dictionary<string, Transform> boneMap, string boneName)
		{
			if (property == null)
			{
				return;
			}

			if (boneName == null || !boneMap.TryGetValue(boneName, out Transform bone))
			{
				property.objectReferenceValue = null;
				return;
			}

			property.objectReferenceValue = bone;
		}

		private void SetObjectReference(SerializedProperty property, UnityEngine.Object value)
		{
			if (property == null)
			{
				return;
			}

			property.objectReferenceValue = value;
		}
	}
}
