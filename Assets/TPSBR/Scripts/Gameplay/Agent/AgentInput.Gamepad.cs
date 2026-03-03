namespace TPSBR
{
	using UnityEngine;
	using UnityEngine.InputSystem;
	using Fusion.Addons.KCC;

	public sealed partial class AgentInput
	{
		partial void ProcessGamepadInput(bool isInputPoll)
		{
			// Very basic gamepad input, not all actions are implemented.

			Gamepad gamepad = Gamepad.current;
			if (gamepad == null)
				return;

			Vector2 moveDirection = gamepad.leftStick.ReadValue();
			if (moveDirection.IsAlmostZero(0.1f) == false)
			{
				_renderInput.MoveDirection = moveDirection;
			}

			Vector2 lookRotationDelta = gamepad.rightStick.ReadValue();
			if (lookRotationDelta.IsAlmostZero() == false)
			{
				lookRotationDelta = new Vector2(-lookRotationDelta.y, lookRotationDelta.x);
				_renderInput.LookRotationDelta = InputUtility.GetSmoothLookRotationDelta(_smoothLookRotationDelta, lookRotationDelta, Global.RuntimeSettings.Sensitivity, _lookResponsivity);
			}

			_renderInput.Jump          |= gamepad.leftTrigger.isPressed;
			_renderInput.Thrust        |= gamepad.leftTrigger.isPressed;
			_renderInput.Attack        |= gamepad.rightTrigger.isPressed;
			_renderInput.Interact      |= gamepad.aButton.isPressed;
			_renderInput.Reload        |= gamepad.xButton.isPressed;
			_renderInput.ToggleJetpack |= gamepad.bButton.isPressed;
			_renderInput.ToggleSide    |= gamepad.yButton.isPressed;
		}
	}
}
