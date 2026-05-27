using Fusion;
using UnityEngine;

namespace TPSBR.Examples
{
    public class TeamFriendlyFirePrevention : MonoBehaviour
    {
        public static bool ShouldApplyDamage(PlayerRef instigator, PlayerRef victim, NetworkGame networkGame)
        {
            if (!instigator.IsRealPlayer || !victim.IsRealPlayer)
                return true;

            if (instigator == victim)
                return true;

            if (TeamManager.Instance == null || networkGame == null)
                return true;

            var instigatorPlayer = networkGame.GetPlayer(instigator);
            var victimPlayer = networkGame.GetPlayer(victim);

            if (instigatorPlayer == null || victimPlayer == null)
                return true;

            if (TeamManager.Instance.AreTeammates(instigatorPlayer.UserID, victimPlayer.UserID))
            {
                return false;
            }

            return true;
        }

        public static bool CanDamageTeammate(Player instigator, Player victim)
        {
            if (instigator == null || victim == null)
                return true;

            if (instigator == victim)
                return true;

            if (TeamManager.Instance == null)
                return true;

            return !TeamManager.Instance.AreTeammates(instigator.UserID, victim.UserID);
        }

        public static Color GetTeamColor(byte teamID)
        {
            Color[] teamColors = new Color[]
            {
                Color.blue,
                Color.green,
                Color.yellow,
                Color.cyan,
                Color.magenta,
                new Color(1f, 0.5f, 0f),
                new Color(0.5f, 0f, 1f),
                new Color(0f, 1f, 0.5f)
            };

            if (teamID > 0 && teamID <= teamColors.Length)
            {
                return teamColors[teamID - 1];
            }

            return Color.white;
        }

        public static string GetTeamName(byte teamID)
        {
            string[] teamNames = new string[]
            {
                "Alpha",
                "Bravo",
                "Charlie",
                "Delta",
                "Echo",
                "Foxtrot",
                "Golf",
                "Hotel"
            };

            if (teamID > 0 && teamID <= teamNames.Length)
            {
                return $"Team {teamNames[teamID - 1]}";
            }

            return $"Team {teamID}";
        }
    }
}
