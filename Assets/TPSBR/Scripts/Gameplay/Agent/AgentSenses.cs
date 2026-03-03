using Fusion;
using UnityEngine;
using DG.Tweening;

namespace TPSBR
{
	public sealed class AgentSenses : NetworkBehaviour
	{
		[Networked, HideInInspector]
		public float  EyesFlashValue { get; set; }

		[SerializeField]
		private Ease  _eyesFlashFalloff;

		private float _eyesFlashStartValue;
		private int   _eyesFlashStartTick;
		private int   _eyesFlashEndTick;

		public void SetEyesFlash(float value, float duration, float falloffDelay)
		{
			if (HasStateAuthority == false)
				return;

			if (falloffDelay > duration - 0.1f)
			{
				falloffDelay = duration - 0.1f;
			}

			_eyesFlashStartTick = Runner.Tick + (int)(falloffDelay / Runner.DeltaTime);
			_eyesFlashEndTick   = _eyesFlashStartTick + (int)(duration / Runner.DeltaTime);

			_eyesFlashStartValue = value;
			EyesFlashValue       = value;
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			var currentTick = Runner.Tick;
			if (currentTick >= _eyesFlashEndTick)
			{
				EyesFlashValue = 0f;
				return;
			}

			var progress   = (currentTick - _eyesFlashStartTick) / (float)(_eyesFlashEndTick - _eyesFlashStartTick);
			EyesFlashValue = Mathf.Lerp(_eyesFlashStartValue, 0f, DOVirtual.EasedValue(0f, 1f, progress, _eyesFlashFalloff));
		}
	}
}
