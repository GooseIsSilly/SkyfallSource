namespace TPSBR
{
	using System;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.SceneManagement;
	using Fusion;
	using TMPro;

	public sealed class Loader : MonoBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private string                  _batchModeScene;
		[SerializeField]
		private bool                    _simulateBatchMode;
		[SerializeField]
		private StandaloneConfiguration _batchModeConfiguration;

		[Header("Startup UI")]
		[SerializeField]
		private GameObject              _pressSpaceUI;
		[SerializeField]
		private bool                    _skipStartupInput = false;
		[SerializeField]
		private float                   _loaderDelay = 5.0f;

		private bool  _isStarting = false;
		private float _timer = 0f;
		private bool  _isDelaying = false;

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			if (Application.isBatchMode == true || _simulateBatchMode == true)
			{
				StartBatchGame();
			}
			else
			{
				if (_skipStartupInput)
				{
					_isDelaying = true;
					_timer = _loaderDelay;
				}
				else
				{
					if (_pressSpaceUI != null)
						_pressSpaceUI.SetActive(true);
				}
			}
		}

		private void Update()
		{
			if (_isStarting || Application.isBatchMode || _simulateBatchMode)
				return;

			if (_isDelaying)
			{
				_timer -= Time.deltaTime;
				if (_timer <= 0f)
				{
					_isStarting = true;
					_isDelaying = false;
					LoadMenu();
				}
				return;
			}

			if (_skipStartupInput)
				return;

			if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
			{
				_isStarting = true;
				if (_pressSpaceUI != null)
					_pressSpaceUI.SetActive(false);
				
				LoadLoader();
			}
		}

		// PRIVATE METHODS

		private void LoadLoader()
		{
			SceneManager.LoadScene(Global.Settings.LoaderScene);
		}

		private void LoadMenu()
		{
			SceneManager.LoadScene(Global.Settings.MenuScene);
		}

		private void StartBatchGame()
		{
			Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
			keyboard.MakeCurrent();

			Mouse mouse = InputSystem.AddDevice<Mouse>();
			mouse.MakeCurrent();

			PlayerData playerData = Global.PlayerService.PlayerData;
			playerData.AgentID  = Global.Settings.Agent.Agents.GetRandom().ID;
			playerData.Nickname = "Batch" + UnityEngine.Random.Range(1000, 10000);

			if (ApplicationSettings.IsQuickPlay == true)
			{
				LoadMenu();
				return;
			}

			if (ApplicationSettings.IsHost == true || ApplicationSettings.IsServer == true)
			{
				_batchModeConfiguration.ExtraPeers = 0;
			}

			string sessionName = ApplicationSettings.SessionName;
			if (ApplicationSettings.HasSessionName == true && sessionName == "random")
			{
				sessionName = Guid.NewGuid().ToString().ToLowerInvariant();
			}

			if (ApplicationSettings.IsHost         == true) { _batchModeConfiguration.GameMode      = GameMode.Host;                    }
			if (ApplicationSettings.IsServer       == true) { _batchModeConfiguration.GameMode      = GameMode.Server;                  }
			if (ApplicationSettings.IsClient       == true) { _batchModeConfiguration.GameMode      = GameMode.Client;                  }
			if (ApplicationSettings.IsBattleRoyale == true) { _batchModeConfiguration.GameplayType  = EGameplayType.BattleRoyale;       }
			if (ApplicationSettings.HasRegion      == true) { _batchModeConfiguration.Region        = ApplicationSettings.Region;       }
			if (ApplicationSettings.HasExtraPeers  == true) { _batchModeConfiguration.ExtraPeers    = ApplicationSettings.ExtraPeers;   }
			if (ApplicationSettings.HasServerName  == true) { _batchModeConfiguration.ServerName    = ApplicationSettings.ServerName;   }
			if (ApplicationSettings.HasMaxPlayers  == true) { _batchModeConfiguration.MaxPlayers    = ApplicationSettings.MaxPlayers;   }
			if (ApplicationSettings.HasSessionName == true) { _batchModeConfiguration.SessionName   = sessionName;                      }
			if (ApplicationSettings.HasCustomLobby == true) { _batchModeConfiguration.CustomLobby   = ApplicationSettings.CustomLobby;  }
			if (ApplicationSettings.HasIPAddress   == true) { _batchModeConfiguration.IPAddress     = ApplicationSettings.IPAddress;    }
			if (ApplicationSettings.HasPort        == true) { _batchModeConfiguration.Port          = (ushort)ApplicationSettings.Port; }
			if (ApplicationSettings.UseMultiplay   == true) { _batchModeConfiguration.Multiplay     = true;                             }
			if (ApplicationSettings.UseMatchmaking == true) { _batchModeConfiguration.Matchmaking   = true;                             }
			if (ApplicationSettings.UseBackfill    == true) { _batchModeConfiguration.Backfill      = true;                             }
			if (ApplicationSettings.UseSQP         == true) { _batchModeConfiguration.QueryProtocol = true;                             }

			StandaloneManager.ExternalConfiguration = _batchModeConfiguration;

			SceneManager.LoadScene(ApplicationSettings.HasCustomScene == true ? ApplicationSettings.CustomScene : _batchModeScene);
		}
	}
}
