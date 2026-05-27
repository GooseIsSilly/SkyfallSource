using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TPSBREditor
{
	public class FixReplayViewerViewportAlpha
	{
		[MenuItem("TPSBR/Fix ReplayViewer Viewport Alpha")]
		public static void FixViewportAlpha()
		{
			Scene replayScene = SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/ReplayViewer.unity");
			
			if (!replayScene.isLoaded)
			{
				replayScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/ReplayViewer.unity", OpenSceneMode.Additive);
			}

			GameObject canvas = GameObject.Find("Canvas");
			if (canvas == null)
			{
				Debug.LogError("Canvas not found in ReplayViewer scene!");
				return;
			}

			Transform replayViewerUI = canvas.transform.Find("ReplayViewerUI");
			if (replayViewerUI == null)
			{
				Debug.LogError("ReplayViewerUI not found!");
				return;
			}

			Transform viewport = replayViewerUI.Find("LeftPanel/ScrollView/Viewport");
			if (viewport == null)
			{
				Debug.LogError("Viewport not found!");
				return;
			}

			Image viewportImage = viewport.GetComponent<Image>();
			if (viewportImage != null)
			{
				Color color = viewportImage.color;
				color.a = 1f;
				viewportImage.color = color;
				Debug.Log($"Fixed Viewport alpha from {viewportImage.color.a} to 1.0");
			}
			else
			{
				Debug.LogWarning("Viewport doesn't have an Image component");
			}

			EditorSceneManager.MarkSceneDirty(replayScene);
			EditorSceneManager.SaveScene(replayScene);

			Debug.Log("ReplayViewer Viewport alpha fixed and scene saved!");
			EditorUtility.DisplayDialog("Success", 
				"Viewport alpha has been set to 255 (fully opaque)!\n\n" +
				"Replay items should now be visible in the ReplayViewer scene.", 
				"OK");
		}
	}
}
