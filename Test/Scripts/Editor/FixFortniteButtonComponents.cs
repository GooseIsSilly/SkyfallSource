using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TPSBR.UI;

public class FixFortniteButtonComponents : EditorWindow
{
    [MenuItem("Tools/Fortnite Lobby/Fix Button Components")]
    public static void FixButtons()
    {
        GameObject lobbyView = GameObject.Find("UIFortniteLobbyView");
        
        if (lobbyView == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find UIFortniteLobbyView in the scene.\n\n" +
                "Please make sure the Menu scene is open and UIFortniteLobbyView exists.", 
                "OK");
            return;
        }
        
        int fixedCount = 0;
        
        Button[] buttons = lobbyView.GetComponentsInChildren<Button>(true);
        
        foreach (Button button in buttons)
        {
            if (button is UIButton)
            {
                continue;
            }
            
            GameObject buttonObj = button.gameObject;
            
            ColorBlock colors = button.colors;
            Navigation navigation = button.navigation;
            bool interactable = button.interactable;
            Graphic targetGraphic = button.targetGraphic;
            
            DestroyImmediate(button);
            
            UIButton uiButton = buttonObj.AddComponent<UIButton>();
            uiButton.colors = colors;
            uiButton.navigation = navigation;
            uiButton.interactable = interactable;
            uiButton.targetGraphic = targetGraphic;
            
            fixedCount++;
            Debug.Log($"Fixed button: {buttonObj.name}");
        }
        
        EditorUtility.SetDirty(lobbyView);
        
        EditorUtility.DisplayDialog("Success!", 
            $"Fixed {fixedCount} buttons!\n\n" +
            "All Button components have been replaced with UIButton components.\n\n" +
            "Now connect the button references in the UIFortniteLobbyView component.", 
            "OK");
    }
}
