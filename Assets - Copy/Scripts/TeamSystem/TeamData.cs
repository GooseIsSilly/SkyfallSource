using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public enum TeamMode
    {
        Solo = 1,
        Duo = 2,
        Squad = 4
    }

    public struct TeamMember : INetworkStruct
    {
        public PlayerRef PlayerRef;
        public NetworkString<_32> Nickname;
        public byte TeamID;
        public bool IsAlive { get { return _flags.IsBitSet(0); } set { _flags.SetBit(0, value); } }
        public bool IsPartyLeader { get { return _flags.IsBitSet(1); } set { _flags.SetBit(1, value); } }

        private byte _flags;
    }

    [Serializable]
    public class TeamData
    {
        public const byte MAX_TEAM_SIZE = 4;
        public const byte MAX_TEAMS = 25;

        public byte TeamID;
        public List<string> MemberUserIDs = new List<string>(MAX_TEAM_SIZE);
        public List<PlayerRef> MemberRefs = new List<PlayerRef>(MAX_TEAM_SIZE);
        public string PartyLeaderUserID;

        public bool IsFull(TeamMode mode)
        {
            return MemberUserIDs.Count >= (int)mode;
        }

        public bool HasMember(string userID)
        {
            return MemberUserIDs.Contains(userID);
        }

        public bool AddMember(string userID)
        {
            if (MemberUserIDs.Count >= MAX_TEAM_SIZE)
                return false;

            if (!MemberUserIDs.Contains(userID))
            {
                MemberUserIDs.Add(userID);
                if (string.IsNullOrEmpty(PartyLeaderUserID))
                {
                    PartyLeaderUserID = userID;
                }
                return true;
            }
            return false;
        }

        public bool RemoveMember(string userID)
        {
            bool removed = MemberUserIDs.Remove(userID);
            if (removed && PartyLeaderUserID == userID)
            {
                PartyLeaderUserID = MemberUserIDs.Count > 0 ? MemberUserIDs[0] : null;
            }
            return removed;
        }

        public bool IsPartyLeader(string userID)
        {
            return PartyLeaderUserID == userID;
        }
    }

    [Serializable]
    public class FriendData
    {
        public string UserID;
        public string Nickname;
        public bool IsOnline;
        public bool InLobby;
        public string CurrentTeamID;
    }

    public class FriendsList
    {
        private const string FRIENDS_KEY = "FRIENDS_LIST";

        public List<FriendData> Friends { get; private set; } = new List<FriendData>();

        public void Load()
        {
            string json = PlayerPrefs.GetString(FRIENDS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<FriendsListWrapper>(json);
                Friends = wrapper.friends ?? new List<FriendData>();
            }
        }

        public void Save()
        {
            var wrapper = new FriendsListWrapper { friends = Friends };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(FRIENDS_KEY, json);
            PlayerPrefs.Save();
        }

        public bool AddFriend(string userID, string nickname)
        {
            if (Friends.Exists(f => f.UserID == userID))
                return false;

            Friends.Add(new FriendData
            {
                UserID = userID,
                Nickname = nickname,
                IsOnline = false,
                InLobby = false
            });
            Save();
            return true;
        }

        public bool RemoveFriend(string userID)
        {
            int removed = Friends.RemoveAll(f => f.UserID == userID);
            if (removed > 0)
            {
                Save();
                return true;
            }
            return false;
        }

        public FriendData GetFriend(string userID)
        {
            return Friends.Find(f => f.UserID == userID);
        }

        [Serializable]
        private class FriendsListWrapper
        {
            public List<FriendData> friends;
        }
    }
}
