using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR.Examples
{
    public class ReviveSystemDebugger : ContextBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField]
        private bool _enableDebugKeys = true;
        [SerializeField]
        private KeyCode _forceDownKey = KeyCode.K;
        [SerializeField]
        private KeyCode _forceReviveKey = KeyCode.L;

        private Player _localPlayer;

        private void Update()
        {
            if (!_enableDebugKeys)
                return;

            if (_localPlayer == null)
            {
                FindLocalPlayer();
            }

            if (_localPlayer == null)
                return;

            if (Keyboard.current != null)
            {
                if (Keyboard.current[Key.K].wasPressedThisFrame)
                {
                    ForceDownState();
                }

                if (Keyboard.current[Key.L].wasPressedThisFrame)
                {
                    ForceRevive();
                }

                if (Keyboard.current[Key.P].wasPressedThisFrame)
                {
                    PrintReviveStatus();
                }
            }
        }

        private void FindLocalPlayer()
        {
            if (Context == null || Context.NetworkGame == null)
                return;

            _localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
        }

        private void ForceDownState()
        {
            if (_localPlayer == null)
                return;

            var reviveSystem = _localPlayer.GetComponent<ReviveSystem>();
            if (reviveSystem != null)
            {
                reviveSystem.EnterDownedState();
                Debug.Log($"[DEBUG] Forced {_localPlayer.Nickname} into downed state");
            }
        }

        private void ForceRevive()
        {
            if (_localPlayer == null)
                return;

            var reviveSystem = _localPlayer.GetComponent<ReviveSystem>();
            if (reviveSystem != null && reviveSystem.IsDown)
            {
                var networkGame = FindObjectOfType<NetworkGame>();
                if (networkGame != null)
                {
                    foreach (var player in networkGame.ActivePlayers)
                    {
                        if (player != _localPlayer && player.IsTeammateWith(_localPlayer))
                        {
                            reviveSystem.StartRevive(player.Object.InputAuthority);
                            Debug.Log($"[DEBUG] Started revive from {player.Nickname}");
                            return;
                        }
                    }
                }
            }
        }

        private void PrintReviveStatus()
        {
            var networkGame = FindObjectOfType<NetworkGame>();
            if (networkGame == null)
                return;

            Debug.Log("=== REVIVE SYSTEM STATUS ===");

            foreach (var player in networkGame.ActivePlayers)
            {
                var reviveSystem = player.GetComponent<ReviveSystem>();
                if (reviveSystem != null)
                {
                    Debug.Log($"{player.Nickname}: Down={reviveSystem.IsDown}, " +
                             $"Being Revived={reviveSystem.IsBeingRevived}, " +
                             $"Bleed Out={reviveSystem.BleedOutProgress:F1}s");
                }
            }
        }

        private void OnGUI()
        {
            if (!_enableDebugKeys)
                return;

            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Label("=== Revive Debug ===");
            GUILayout.Label($"Press K - Force Down State");
            GUILayout.Label($"Press L - Force Revive");
            GUILayout.Label($"Press P - Print Status");

            if (_localPlayer != null)
            {
                var reviveSystem = _localPlayer.GetComponent<ReviveSystem>();
                if (reviveSystem != null && reviveSystem.IsDown)
                {
                    GUILayout.Label($"Status: DOWNED");
                    GUILayout.Label($"Bleed Out: {reviveSystem.BleedOutProgress:F1}s");
                    GUILayout.Label($"Revive Progress: {reviveSystem.ReviveProgress * 100:F0}%");
                }
            }

            GUILayout.EndArea();
        }
    }
}
