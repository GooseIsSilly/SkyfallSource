namespace TPSBR
{
	using UnityEngine;
	using TPSBR.UI;

	public sealed partial class AgentInput
	{
		partial void ProcessMobileInput(bool isInputPoll)
		{
			// Very basic mobile input, not all actions are implemented.

			Vector2 moveDirection;
			Vector2 lookRotationDelta;

			if (_mobileInputView == null)
			{
				if (Context != null && Context.UI != null)
				{
					_mobileInputView = Context.UI.Get<UIMobileInputView>();
				}

				return;
			}

			const float mobileSensitivityMultiplier = 32.0f;

			moveDirection     = _mobileInputView.Move.normalized;
			lookRotationDelta = InputUtility.GetSmoothLookRotationDelta(_smoothLookRotationDelta, new Vector2(-_mobileInputView.Look.y, _mobileInputView.Look.x) * mobileSensitivityMultiplier, Global.RuntimeSettings.Sensitivity, _lookResponsivity);

			_mobileInputView.Look = default;

			if (_agent.Character.CharacterController.FixedData.Aim == true)
			{
				lookRotationDelta *= Global.RuntimeSettings.AimSensitivity;
			}

			_renderInput.MoveDirection     = moveDirection;
			_renderInput.LookRotationDelta = lookRotationDelta;
			_renderInput.Jump              = _mobileInputView.Jump;
			_renderInput.Attack            = _mobileInputView.Fire;
			_renderInput.Interact          = _mobileInputView.Interact;
		}
	}
}
