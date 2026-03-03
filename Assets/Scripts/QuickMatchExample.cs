using UnityEngine;
using TPSBR.UI;

public class QuickMatchExample : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        var multiplayerView = FindObjectOfType<UIMultiplayerView>();
        if (multiplayerView != null)
        {
            multiplayerView.StartQuickMatch();
        }
        else
        {
            Debug.LogWarning("UIMultiplayerView not found. Make sure you're in the menu scene.");
        }
    }
}
