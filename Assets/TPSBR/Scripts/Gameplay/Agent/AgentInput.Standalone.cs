using Fusion.Addons.KCC;

namespace TPSBR
{
	using UnityEngine;
	using UnityEngine.InputSystem;

	public sealed partial class AgentInput
	{
		partial void ProcessStandaloneInput(bool isInputPoll)
		{
			// Always use KeyControl.isPressed, Input.GetMouseButton() and Input.GetKey().
			// Never use KeyControl.wasPressedThisFrame, Input.GetMouseButtonDown() or Input.GetKeyDown() otherwise the action might be lost.

			Vector2 moveDirection;
			Vector2 lookRotationDelta;

			Mouse    mouse      = Mouse.current;
			Keyboard keyboard   = Keyboard.current;
			Vector2  mouseDelta = mouse.delta.ReadValue() * 0.075f;

			moveDirection     = Vector2.zero;
			lookRotationDelta = InputUtility.GetSmoothLookRotationDelta(_smoothLookRotationDelta, new Vector2(-mouseDelta.y, mouseDelta.x), Global.RuntimeSettings.Sensitivity, _lookResponsivity);

			if (_agent.Character.CharacterController.FixedData.Aim == true)
			{
				lookRotationDelta *= Global.RuntimeSettings.AimSensitivity;
			}

			if (keyboard.wKey.isPressed == true) { moveDirection += Vector2.up;    }
			if (keyboard.sKey.isPressed == true) { moveDirection += Vector2.down;  }
			if (keyboard.aKey.isPressed == true) { moveDirection += Vector2.left;  }
			if (keyboard.dKey.isPressed == true) { moveDirection += Vector2.right; }

			if (moveDirection.IsZero() == false)
			{
				moveDirection.Normalize();
			}

			_renderInput.MoveDirection     = moveDirection;
			_renderInput.LookRotationDelta = lookRotationDelta;
			_renderInput.Jump              = keyboard.spaceKey.isPressed;
			_renderInput.Aim               = mouse.rightButton.isPressed;
			_renderInput.Attack            = mouse.leftButton.isPressed;
			_renderInput.Reload            = keyboard.rKey.isPressed;
			_renderInput.Interact          = keyboard.fKey.isPressed;
			_renderInput.Weapon            = GetWeaponInput(keyboard);
			_renderInput.ToggleJetpack     = keyboard.xKey.isPressed;
			_renderInput.Thrust            = keyboard.spaceKey.isPressed;
			_renderInput.Sprint            = keyboard.leftShiftKey.isPressed;
			_renderInput.ToggleSide        = keyboard.eKey.isPressed;
		}
	}
}
