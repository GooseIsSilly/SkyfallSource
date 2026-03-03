using UnityEngine;

namespace TPSBR
{
    public static class PlayerReviveExtension
    {
        public static bool IsDown(this Player player)
        {
            var reviveSystem = player.GetComponent<ReviveSystem>();
            return reviveSystem != null && reviveSystem.IsDown;
        }

        public static bool IsBeingRevived(this Player player)
        {
            var reviveSystem = player.GetComponent<ReviveSystem>();
            return reviveSystem != null && reviveSystem.IsBeingRevived;
        }

        public static float GetBleedOutProgress(this Player player)
        {
            var reviveSystem = player.GetComponent<ReviveSystem>();
            return reviveSystem != null ? reviveSystem.BleedOutProgress : 0f;
        }

        public static float GetReviveProgress(this Player player)
        {
            var reviveSystem = player.GetComponent<ReviveSystem>();
            return reviveSystem != null ? reviveSystem.ReviveProgress : 0f;
        }

        public static Player GetReviver(this Player player)
        {
            var reviveSystem = player.GetComponent<ReviveSystem>();
            return reviveSystem != null ? reviveSystem.GetRevivingPlayer() : null;
        }

        public static ReviveSystem GetReviveSystem(this Player player)
        {
            return player.GetComponent<ReviveSystem>();
        }
    }
}
