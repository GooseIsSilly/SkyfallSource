using System;
using System.Diagnostics;
using Fusion;
using UnityEngine;

namespace TPSBR
{
	public interface IPlayer
	{
		string           UserID      { get; }
		string           Nickname    { get; }
		NetworkPrefabRef AgentPrefab { get; }
		string			 UnityID     { get; }
	}

	public class PlayerData : IPlayer
	{
		// PUBLIC MEMBERS

		public string           UserID          => _userID;
		public string           UnityID         { get => _unityID; set => _unityID = value; }
		public NetworkPrefabRef AgentPrefab     => GetAgentPrefab();
		public string           Nickname        { get { return _nickname; } set { _nickname = value; IsDirty = true; } }
		public string           AgentID         { get { return _agentID; } set { _agentID = value; IsDirty = true; } }

		public CloudCoinSystem  CoinSystem      => _coinSystem;
		public ShopSystem       ShopSystem      => _shopSystem;

		public bool             IsDirty         { get; private set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private string _userID;
		[SerializeField]
		private string _unityID;
		[SerializeField]
		private string _nickname;
		[SerializeField]
		private string _agentID;

		[SerializeField]
		private CloudCoinSystem _coinSystem = new CloudCoinSystem();
		[SerializeField]
		private ShopSystem _shopSystem = new ShopSystem();

		[SerializeField]
		private bool _isLocked;
		[SerializeField]
		private int _lastProcessID;

		// CONSTRUCTORS

		public PlayerData(string userID)
	{
		_userID = userID;
		
		var shopDatabase = Global.Settings != null ? Global.Settings.ShopDatabase : null;
		if (shopDatabase != null)
		{
			_shopSystem.InitializeWithDatabase(shopDatabase);
			
			if (_coinSystem.CloudCoins == 100)
			{
				_coinSystem.CloudCoins = shopDatabase.startingCloudCoins;
			}
		}
		else
		{
			_shopSystem.Initialize();
		}
	}

	// PUBLIC METHODS

		public void ClearDirty()
		{
			IsDirty = false;
			_coinSystem.ClearDirty();
			_shopSystem.ClearDirty();
		}

		public bool IsLocked(bool checkProcess = true)
		{
			if (_isLocked == false)
				return false;

			if (checkProcess == true)
			{
				try
				{
					var process = Process.GetProcessById(_lastProcessID);
				}
				catch (Exception)
				{
					// Process not running
					return false;
				}
			}

			return true;
		}

		public void Lock()
		{
			// When running multiple instances of the game on same machine we want to lock used player data

			_isLocked = true;
			_lastProcessID = Process.GetCurrentProcess().Id;
		}

		public void Unlock()
		{
			_isLocked = false;
		}

		// PRIVATE METHODS

		private NetworkPrefabRef GetAgentPrefab()
		{
			if (_agentID.HasValue() == false)
				return default;

			var setup = Global.Settings.Agent.GetAgentSetup(_agentID);
			return setup != null ? setup.AgentPrefab : default;
		}
	}
}
