namespace TPSBR
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Fusion.Photon.Realtime;

	public static class ProjectUtility
	{
		// PUBLIC METHODS

		[MenuItem("BR200/Prepare Regular Build")]
		public static void PrepareRegularBuild()
		{
			PhotonAppSettings.Global.AppSettings.AppVersion = $"{Application.version}-{DateTime.Now.ToString("yyMMdd")}";
			EditorUtility.SetDirty(PhotonAppSettings.Global);

			GlobalSettings globalSettings = Resources.LoadAll<GlobalSettings>("")[0];
			globalSettings.Network.QueueName = $"Queue-Build-{DateTime.Now.ToString("yyMMdd")}";
			EditorUtility.SetDirty(globalSettings.Network);

			AssetDatabase.SaveAssets();
		}

		[MenuItem("BR200/Prepare Public Build")]
		public static void PreparePublicBuild()
		{
			PhotonAppSettings.Global.AppSettings.AppVersion = $"{Application.version}-{DateTime.Now.ToString("yyMMdd")}-public";
			EditorUtility.SetDirty(PhotonAppSettings.Global);

			GlobalSettings globalSettings = Resources.LoadAll<GlobalSettings>("")[0];
			globalSettings.Network.QueueName = $"Queue-Build-{DateTime.Now.ToString("yyMMdd")}";
			EditorUtility.SetDirty(globalSettings.Network);

			AssetDatabase.SaveAssets();
		}

		[MenuItem("BR200/Reserialize All Prefabs")]
		public static void ReserializeAllPrefabs()
		{
			int count = 0;

			foreach (string prefabPath in GetAllAssets(".prefab"))
			{
				GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				if (PrefabUtility.IsPartOfImmutablePrefab(prefabAsset) == false)
				{
					Debug.Log($"Reserializing {prefabPath}", prefabAsset);
					PrefabUtility.SavePrefabAsset(prefabAsset);
					++count;
				}
			}

			Debug.LogWarning($"Reserialized {count} prefabs");
		}

		// PRIVATE METHODS

		private static string[] GetAllAssets(string suffix)
		{
			List<string> assetPaths = new List<string>();
			foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
			{
				if (assetPath.EndsWith(suffix) == true)
				{
					assetPaths.Add(assetPath);
				}
			}

			return assetPaths.ToArray();
		}
	}
}
