using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TPSBREditor
{
	public class FixReplayGalleryHierarchy : EditorWindow
	{
		[MenuItem("TPSBR/Fix Replay Gallery Hierarchy Order")]
		public static void FixHierarchy()
		{
			string menuScenePath = "Assets/TPSBR/Scenes/Menu.unity";
			if (!System.IO.File.Exists(menuScenePath))
			{
				EditorUtility.DisplayDialog("Error", "Menu scene not found!", "OK");
				return;
			}

			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.OpenScene(menuScenePath);
				
				GameObject menuUI = GameObject.Find("MenuUI");
				if (menuUI == null)
				{
					EditorUtility.DisplayDialog("Error", "MenuUI not found in Menu scene!", "OK");
					return;
				}

				Transform replayGallery = menuUI.transform.Find("UIReplayGalleryView");
				if (replayGallery == null)
				{
					EditorUtility.DisplayDialog("Error", 
						"UIReplayGalleryView not found in Menu scene!\n\n" +
						"Add it to the scene first using:\n" +
						"TPSBR → Replay System Debugger → Add to Menu Scene", 
						"OK");
					return;
				}

				replayGallery.SetAsLastSibling();

				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				EditorSceneManager.SaveOpenScenes();

				EditorUtility.DisplayDialog("Fixed!", 
					"UIReplayGalleryView has been moved to render on top!\n\n" +
					"The replay gallery will now appear above all other UI elements.\n" +
					"Buttons should work correctly now.", 
					"OK");

				Debug.Log("[ReplayGallery] Moved UIReplayGalleryView to last sibling position");
			}
		}
	}
}
