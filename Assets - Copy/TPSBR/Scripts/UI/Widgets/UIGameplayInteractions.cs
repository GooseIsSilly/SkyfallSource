using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
	public class UIGameplayInteractions : UIBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private UIBehaviour _interactGroup;
		[SerializeField]
		private UIBehaviour _interactionInfoGroup;
		[SerializeField]
		private TextMeshProUGUI _interactionName;
		[SerializeField]
		private TextMeshProUGUI _interactionDescription;
		[SerializeField]
		private Transform _dropWeaponGroup;
		[SerializeField]
		private Image _dropWeaponProgress;

		private bool _hasInteractionTarget;
		private IInteraction _interactionTarget;
		private bool _infoActive;

		// MONOBEHAVIOUR

		protected void OnEnable()
		{
			SetInteractionTarget(null, true);
		}

		// PUBLIC MEMBERS

		public void UpdateInteractions(SceneContext context, Agent agent)
		{
			var interactionTarget = agent.Interactions.InteractionTarget;
			bool force = false;

			// Interaction target could get destroyed in the meantime (+ special check due to interface required)
			if (_hasInteractionTarget == true && (interactionTarget == null || interactionTarget.Equals(null)))
			{
				interactionTarget = null;
				force = true;
			}

			SetInteractionTarget(interactionTarget, force);

			UpdateInfoPosition(context);

			if (agent.Interactions.DropItemTimer.IsRunning == false)
			{
				_dropWeaponGroup.SetActive(false);
			}
			else
			{
				_dropWeaponGroup.SetActive(true);
				_dropWeaponProgress.fillAmount = 1f - (agent.Interactions.DropItemTimer.RemainingTime(agent.Runner).Value / agent.Interactions.ItemDropTime);
			}
		}

		// PRIVATE MEMBERS

		private void SetInteractionTarget(IInteraction interactionTarget, bool force = false)
		{
			if (interactionTarget == _interactionTarget && force == false)
				return;

			_interactionTarget = interactionTarget;
			_hasInteractionTarget = interactionTarget != null;

			_interactGroup.SetActive(_hasInteractionTarget);

			_infoActive = _hasInteractionTarget == true && interactionTarget.Name.HasValue();
			_interactionInfoGroup.SetActive(_infoActive);

			if (_infoActive == true)
			{
				_interactionName.text = interactionTarget.Name;
				_interactionDescription.text = interactionTarget.Description;
			}
		}

		private void UpdateInfoPosition(SceneContext context)
		{
			if (_infoActive == false)
				return;

			var screenPosition = context.Camera.Camera.WorldToScreenPoint(_interactionTarget.HUDPosition);
			_interactionInfoGroup.transform.position = screenPosition;
		}
	}
}
