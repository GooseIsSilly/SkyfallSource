using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

namespace TPSBR.UI
{
	public class UIAgentSelectionView : UICloseView
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private CinemachineVirtualCamera _camera;
		[SerializeField]
		private UIList _agentList;
		[SerializeField]
		private UIButton _selectButton;
		[SerializeField]
		private TextMeshProUGUI _agentName;
		[SerializeField]
		private TextMeshProUGUI _agentDescription;
		[SerializeField]
		private string _agentNameFormat = "{0}";
		[SerializeField]
		private UIBehaviour _selectedAgentGroup;
		[SerializeField]
		private UIBehaviour _selectedEffect;
		[SerializeField]
		private AudioSetup _selectedSound;
		[SerializeField]
		private float _closeDelayAfterSelection = 0.5f;

		private string _previewAgent;

		// UIView INTERFACE

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_agentList.SelectionChanged += OnSelectionChanged;
			_agentList.UpdateContent += OnListUpdateContent;

			_selectButton.onClick.AddListener(OnSelectButton);
		}

		private void OnListUpdateContent(int index, MonoBehaviour content)
		{
			var agentSetups = GetOwnedAgents();

			if (index >= agentSetups.Length)
				return;

			var setup = agentSetups[index];

			// The icon Image may be on the content object itself or on a child — search both.
			var image = content.GetComponent<Image>();
			if (image == null)
			{
				image = content.GetComponentInChildren<Image>(true);
			}

			if (image != null)
			{
				image.sprite = setup.Icon;
			}
			else
			{
				Debug.LogWarning($"[UIAgentSelectionView] No Image component found on agent list item content at index {index}.", content);
			}
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			if (_camera != null)
			{
				_camera.enabled = true;
			}

			if (_selectedEffect != null)
			{
				_selectedEffect.SetActive(false);
			}

			_previewAgent = Context.PlayerData.AgentID;

			var ownedAgents = GetOwnedAgents();
			_agentList.Refresh(ownedAgents.Length, false);

			// Set selection without triggering SelectionChanged to avoid redundant UpdateAgent calls
			int selectionIndex = Array.FindIndex(ownedAgents, t => t.ID == _previewAgent);
			if (selectionIndex < 0 && ownedAgents.Length > 0)
			{
				selectionIndex = 0;
				_previewAgent = ownedAgents[0].ID;
			}
			_agentList.Selection = selectionIndex;
			
			UpdateAgent();
		}

		protected override void OnClose()
		{
			if (_camera != null)
			{
				_camera.enabled = false;
			}

			Context.PlayerPreview.ShowAgent(Context.PlayerData.AgentID);

			base.OnClose();
		}

		protected override void OnDeinitialize()
		{
			_agentList.SelectionChanged -= OnSelectionChanged;
			_agentList.UpdateContent -= OnListUpdateContent;

			_selectButton.onClick.RemoveListener(OnSelectButton);

			base.OnDeinitialize();
		}

		// PRIVATE METHODS

		private void OnSelectionChanged(int index)
		{
			var ownedAgents = GetOwnedAgents();
			if (index >= 0 && index < ownedAgents.Length)
			{
				_previewAgent = ownedAgents[index].ID;
				UpdateAgent();
			}
		}

		private void OnSelectButton()
		{
			bool isSame = Context.PlayerData.AgentID == _previewAgent;

			if (isSame == false)
			{
				Context.PlayerData.AgentID = _previewAgent;

				if (_selectedEffect != null)
				{
					_selectedEffect.SetActive(false);
					_selectedEffect.SetActive(true);
				}

				PlaySound(_selectedSound);

				UpdateAgent();
				Invoke("CloseWithBack", _closeDelayAfterSelection);
			}
			else
			{
				CloseWithBack();
			}
		}

		private void UpdateAgent()
		{
			var ownedAgents = GetOwnedAgents();
			int selectionIndex = Array.FindIndex(ownedAgents, t => t.ID == _previewAgent);

			if (selectionIndex < 0 && ownedAgents.Length > 0)
			{
				selectionIndex = 0;
				_previewAgent = ownedAgents[0].ID;
			}

			_agentList.Selection = selectionIndex;

			if (_previewAgent.HasValue() == false)
			{
				Context.PlayerPreview.HideAgent();
				_agentName.text = string.Empty;
				_agentDescription.text = string.Empty;
			}
			else
			{
				var setup = Context.Settings.Agent.GetAgentSetup(_previewAgent);

				Context.PlayerPreview.ShowAgent(_previewAgent);
				_agentName.text = string.Format(_agentNameFormat, setup.DisplayName);
				_agentDescription.text = setup.Description;
			}

			_selectedAgentGroup.SetActive(_previewAgent == Context.PlayerData.AgentID);
		}

		private AgentSetup[] GetOwnedAgents()
		{
			var allAgents = Context.Settings.Agent.Agents;
			var ownedAgentsList = new System.Collections.Generic.List<AgentSetup>();

			foreach (var agent in allAgents)
			{
				if (Context.PlayerData.ShopSystem.OwnsAgent(agent.ID))
				{
					ownedAgentsList.Add(agent);
				}
			}

			return ownedAgentsList.ToArray();
		}
	}
}
