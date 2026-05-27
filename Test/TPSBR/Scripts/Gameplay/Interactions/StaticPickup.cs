using Fusion;
using UnityEngine;

namespace TPSBR
{
	public abstract class StaticPickup : NetworkBehaviour, IPickup
	{
		// PUBLIC MEMBERS

		public bool Consumed    => _consumed;
		public bool IsDisabled  => _isDisabled;
		public bool AutoDespawn => _despawnDelay > 0f;

		public System.Action<StaticPickup> PickupConsumed;

		// PROTECTED MEMBERS

		[Networked]
		protected EBehaviour _behaviour { get; private set; }
		[Networked]
		protected NetworkBool _consumed { get; private set; }
		[Networked]
		protected NetworkBool _isDisabled { get; private set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private AudioEffect _consumeSound;
		[SerializeField]
		private GameObject _visuals;
		[SerializeField]
		private float _despawnDelay = 2f;
		[SerializeField]
		private Transform _hudPosition;
		[SerializeField]
		private string _interactionName;
		[SerializeField]
		private string _interactionDescription;
		[SerializeField]
		private EBehaviour _startBehaviour;

		private bool       _localIsInitialized;
		private EBehaviour _localBehaviour;
		private bool       _localConsumed;
		private TickTimer  _despawnCooldown;
		private Collider   _collider;

		// IInteraction INTERFACE

		string  IInteraction.Name        => InteractionName;
		string  IInteraction.Description => InteractionDescription;
		Vector3 IInteraction.HUDPosition => _hudPosition != null ? _hudPosition.position : transform.position;
		bool    IInteraction.IsActive    => IsDisabled == false;

		protected virtual string InteractionName        => _interactionName;
		protected virtual string InteractionDescription => _interactionDescription;

		// PUBLIC MEMBERS

		public void Refresh()
		{
			if (Object == null || HasStateAuthority == false)
				return;

			_despawnCooldown = default;
			_consumed        = false;

			SetIsDisabled(false);

			UpdateLocalState();
		}

		public void SetIsDisabled(bool value)
		{
			if (Object == null || HasStateAuthority == false)
				return;

			_isDisabled = value;
		}

		public bool TryConsume(GameObject instigator, out string result)
		{
			if (Object == null)
			{
				result = "No network state";
				return false;
			}

			if (_isDisabled == true || _consumed == true)
			{
				result = "Invalid Pickup";
				return false;
			}

			if (Consume(instigator, out result) == false)
				return false;

			_consumed = true;

			if (_despawnDelay > 0f)
			{
				_despawnCooldown = TickTimer.CreateFromSeconds(Runner, _despawnDelay);
			}

			UpdateLocalState();

			PickupConsumed?.Invoke(this);

			var agent = instigator.GetComponent<Agent>();
			if (agent != null)
			{
				GameplayQuestHooks.NotifyItemPickup(agent);
			}

			return true;
		}

		public void SetBehaviour(EBehaviour behaviour, float despawnDelay)
		{
			if (Object == null || HasStateAuthority == false)
				return;

			_behaviour    = behaviour;
			_despawnDelay = despawnDelay;

			UpdateLocalState();
		}

		// PROTECTED METHODS

		protected virtual bool Consume(GameObject instigator, out string result) { result = string.Empty; return false; }

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_localIsInitialized = default;
			_localBehaviour     = default;
			_localConsumed      = default;

			if (HasStateAuthority == true)
			{
				_behaviour = _startBehaviour;
			}

			UpdateLocalState();
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			PickupConsumed = null;

			_despawnCooldown    = default;
			_localConsumed      = default;
			_localBehaviour     = default;
			_localIsInitialized = default;
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			if (_consumed == true && _despawnCooldown.Expired(Runner) == true)
			{
				Runner.Despawn(Object);
				return;
			}

			UpdateLocalState();
		}

		public override void Render()
		{
			UpdateLocalState();
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_collider = GetComponentInChildren<Collider>();
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (Object == null || HasStateAuthority == false)
				return;

			if (_consumed == true)
				return;

			var networkObject = other.GetComponentInParent<NetworkObject>();
			if (networkObject == null)
				return;

			TryConsume(networkObject.gameObject, out string result);
		}

		// PRIVATE METHODS

		private void UpdateLocalState()
		{
			if (_localIsInitialized == false || _localConsumed != _consumed)
			{
				_localConsumed = _consumed;
				_collider.enabled = _consumed == false;
				_visuals.SetActiveSafe(_consumed == false);

				if (_consumed == true)
				{
					if (_consumeSound != null)
					{
						_consumeSound.Play();
					}
				}
			}

			if (_localIsInitialized == false || _localBehaviour != _behaviour)
			{
				_localBehaviour = _behaviour;
				_collider.isTrigger = _behaviour == EBehaviour.Trigger;
				_collider.gameObject.layer = _behaviour == EBehaviour.Trigger ? ObjectLayer.Pickup : ObjectLayer.Interaction;
			}

			_localIsInitialized = true;
		}

		// HELPERS

		public enum EBehaviour
		{
			None,
			Trigger,
			Interaction,
		}
	}
}
