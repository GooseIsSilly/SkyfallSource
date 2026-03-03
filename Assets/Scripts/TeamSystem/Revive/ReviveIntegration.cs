using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class ReviveIntegration : ContextBehaviour
    {
        [SerializeField]
        private bool _enableReviveSystem = true;

        public override void FixedUpdateNetwork()
        {
            if (!_enableReviveSystem || Context == null || Context.NetworkGame == null)
                return;

            CheckAndAddReviveSystemToPlayers();
        }

        private void CheckAndAddReviveSystemToPlayers()
        {
            var players = Context.NetworkGame.ActivePlayers;
            if (players == null)
                return;

            foreach (var player in players)
            {
                if (player == null)
                    continue;

                var reviveSystem = player.GetComponent<ReviveSystem>();
                if (reviveSystem == null)
                {
                    reviveSystem = player.gameObject.AddComponent<ReviveSystem>();
                    Debug.Log($"[ReviveIntegration] Added ReviveSystem to player {player.Nickname} immediately on network tick");
                }
            }
        }

        public void SetReviveSystemEnabled(bool enabled)
        {
            _enableReviveSystem = enabled;
        }
    }
}
