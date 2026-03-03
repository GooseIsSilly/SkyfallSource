using UnityEngine;
using TPSBR;
using TPSBR.UI;
using Fusion;

public class QuickMatchManager : ContextBehaviour
{
    [Header("Quick Match Settings")]
    [SerializeField] private EGameplayType _gameplayType = EGameplayType.BattleRoyale;
    [SerializeField] private int _maxPlayers = 100;
    [SerializeField] private float _searchTimeout = 5f;
    [SerializeField] private string _defaultMapPath = "TPSBR/Scenes/Game";

    public void StartQuickMatch()
    {
        var multiplayerView = FindObjectOfType<UIMultiplayerView>();
        if (multiplayerView != null)
        {
            multiplayerView.StartQuickMatch();
        }
        else
        {
            Debug.LogWarning("UIMultiplayerView not found. Creating direct quick match...");
            StartDirectQuickMatch();
        }
    }

    private void StartDirectQuickMatch()
    {
        if (Context.Matchmaking == null)
        {
            Debug.LogError("Matchmaking service not available!");
            return;
        }

        var mapSetup = GetMapSetup();
        if (mapSetup == null)
        {
            Debug.LogError("No valid map configuration found!");
            return;
        }

        var request = new SessionRequest
        {
            UserID = Context.PlayerData.UserID,
            GameMode = GameMode.Host,
            DisplayName = $"{Context.PlayerData.Nickname}'s Game",
            SessionName = null,
            ScenePath = mapSetup.ScenePath,
            GameplayType = _gameplayType,
            MaxPlayers = _maxPlayers,
            ExtraPeers = 0,
        };

        Context.Matchmaking.CreateSession(request);
    }

    private MapSetup GetMapSetup()
    {
        var mapSettings = Global.Settings.Map;

        if (mapSettings.Maps != null && mapSettings.Maps.Length > 0)
        {
            foreach (var map in mapSettings.Maps)
            {
                if (map.ScenePath == _defaultMapPath)
                    return map;
            }

            return mapSettings.Maps[0];
        }

        return null;
    }
}
