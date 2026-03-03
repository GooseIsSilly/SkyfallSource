using Fusion;

namespace TPSBR
{
    public static class PlayerTeamExtension
    {
        public static byte GetTeamID(this Player player)
        {
            if (TeamManager.Instance == null || player == null)
                return 0;

            return TeamManager.Instance.GetPlayerTeamID(player.UserID);
        }

        public static bool IsTeammateWith(this Player player, Player otherPlayer)
        {
            if (TeamManager.Instance == null || player == null || otherPlayer == null)
                return false;

            return TeamManager.Instance.AreTeammates(player.UserID, otherPlayer.UserID);
        }

        public static bool IsTeammateWith(this Player player, PlayerRef otherPlayerRef, NetworkGame networkGame)
        {
            if (TeamManager.Instance == null || player == null || networkGame == null)
                return false;

            var otherPlayer = networkGame.GetPlayer(otherPlayerRef);
            if (otherPlayer == null)
                return false;

            return TeamManager.Instance.AreTeammates(player.UserID, otherPlayer.UserID);
        }

        public static TeamData GetTeam(this Player player)
        {
            if (TeamManager.Instance == null || player == null)
                return null;

            return TeamManager.Instance.GetPlayerTeam(player.UserID);
        }

        public static bool IsPartyLeader(this Player player)
        {
            var team = player.GetTeam();
            return team != null && team.IsPartyLeader(player.UserID);
        }
    }
}
