using System;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public sealed class ItemBox : StaticNetworkTransform, IInteraction
    {
        // HELPERS

        public enum EState
        {
            None,
            Closed,
            Open,
            Locked,
        }

        // PRIVATE MEMBERS

        [Header("Item Box")]
        [SerializeField]
        private float _autoCloseTime;
        [SerializeField]
        private float _unlockTime;
        [SerializeField]
        private EState _startState;

        [SerializeField]
        private Transform _lockedState;
        [SerializeField]
        private Transform _unlockedState;

        [SerializeField]
        private AnimationClip _openAnimation;
        [SerializeField]
        private AnimationClip _closeAnimation;

        [Header("Interaction")]
        [SerializeField]
        private string _interactionName;
        [SerializeField]
        private string _interactionDescription;
        [SerializeField]
        private Transform _hudPivot;
        [SerializeField]
        private Collider _interactionCollider;

        [Header("Pickups")]
        [SerializeField]
        private EBehaviour _behaviour;
        [SerializeField]
        private PickupPoint[] _pickupsSetup;

        [Header("Audio")]
        [SerializeField]
        private AudioEffect _audioEffect;
        [SerializeField]
        private AudioSetup _openSound;
        [SerializeField]
        private AudioSetup _closeSound;
        [SerializeField]
        private ItemBoxProximitySound _proximitySound;

        [Networked]
        private EState BoxState { get; set; }
        [Networked]
        private int BoxTimer { get; set; }

        private Animation _animation;
        private StaticPickup[] _nestedPickups;
        private EState _localState;

        // PUBLIC METHODS

        public void Open()
        {
            if (HasStateAuthority == false)
                return;
            if (BoxState != EState.Closed)
                return;

            BoxTimer = GetExpirationTick(_autoCloseTime);
            BoxState = EState.Open;

            if (_proximitySound != null)
            {
                _proximitySound.OnOpened();
            }

            if (_behaviour == EBehaviour.RandomOnOpen || _nestedPickups[0] == null)
            {
                for (int i = 0; i < _nestedPickups.Length; i++)
                {
                    if (_nestedPickups[i] != null)
                    {
                        Runner.Despawn(_nestedPickups[i].Object);
                    }

                    _nestedPickups[i] = SpawnPickup(_pickupsSetup[i]);
                }
            }
            else if (_behaviour == EBehaviour.RandomOnSpawn)
            {
                for (int i = 0; i < _nestedPickups.Length; i++)
                {
                    _nestedPickups[i].Refresh();
                }
            }

            UpdateLocalState();
        }

        // IInteraction INTERFACE

        string IInteraction.Name => _interactionName;
        string IInteraction.Description => _interactionDescription;
        Vector3 IInteraction.HUDPosition => _hudPivot != null ? _hudPivot.position : transform.position;
        bool IInteraction.IsActive => BoxState == EState.Closed;

        // MonoBehaviour INTERFACE

        private void Awake()
        {
            _animation = GetComponent<Animation>();
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            base.Spawned();

            _localState = EState.None;

            if (HasStateAuthority == false)
            {
                if (ApplicationSettings.IsStrippedBatch == true)
                {
                    gameObject.SetActive(false);
                }

                UpdateLocalState();

                return;
            }

            _nestedPickups = new StaticPickup[_pickupsSetup.Length];

            switch (_startState)
            {
                case EState.None:
                case EState.Closed:
                    Unlock();
                    break;
                case EState.Open:
                    Open();
                    break;
                case EState.Locked:
                    Lock();
                    break;
                default:
                    break;
            }

            UpdateLocalState();
        }

        public override void FixedUpdateNetwork()
        {
            // Expired
            if (BoxTimer <= 0 || BoxTimer > Runner.Tick)
                return;

            switch (BoxState)
            {
                case EState.Open: Lock(); break;
                case EState.Locked: Unlock(); break;
            }

            UpdateLocalState();
        }

        public override void Render()
        {
            UpdateLocalState();
        }

        // PRIVATE METHODS

        private void Lock()
        {
            if (BoxState == EState.Locked)
                return;

            for (int i = 0; i < _nestedPickups.Length; i++)
            {
                if (_nestedPickups[i] != null)
                {
                    _nestedPickups[i].SetIsDisabled(true);
                }
            }

            BoxTimer = GetExpirationTick(_unlockTime);
            BoxState = EState.Locked;
        }

        private void Unlock()
        {
            BoxState = EState.Closed;

            for (int i = 0; i < _nestedPickups.Length; i++)
            {
                if (_nestedPickups[i] != null)
                {
                    _nestedPickups[i].SetIsDisabled(true);
                }
            }
        }

        private StaticPickup SpawnPickup(PickupPoint point)
        {
            var prefab = ChoosePickup(point.Pickups);

            if (prefab == null)
            {
                Debug.LogWarning($"[ItemBox] No valid pickup prefab found for ItemBox '{name}'. Check PickupData configuration in Inspector.");
                return null;
            }

            var pickup = Runner.Spawn(prefab, point.Transform.position, point.Transform.rotation);
            pickup.SetBehaviour(StaticPickup.EBehaviour.Interaction, -1f);
            pickup.PickupConsumed += OnPickupConsumed;

            return pickup;
        }

        private void UpdateLocalState()
        {
            if (ApplicationSettings.IsStrippedBatch == true)
                return;

            if (_localState == BoxState)
                return;

            _localState = BoxState;

            _lockedState.SetActive(_localState == EState.Locked);
            _unlockedState.SetActive(_localState != EState.Locked);
            _interactionCollider.enabled = _localState == EState.Closed;

            switch (_localState)
            {
                case EState.Open:
                    if (_proximitySound != null)
                    {
                        _proximitySound.OnOpened();
                    }

                    if (_animation.clip != _openAnimation)
                    {
                        _animation.clip = _openAnimation;
                        _animation.Play();

                        if (_audioEffect != null)
                        {
                            _audioEffect.Play(_openSound, EForceBehaviour.ForceAny);
                        }
                    }
                    break;
                case EState.Closed:
                case EState.Locked:
                    if (_proximitySound != null)
                    {
                        _proximitySound.OnClosed();
                    }

                    if (_animation.clip != _closeAnimation)
                    {
                        _animation.clip = _closeAnimation;
                        _animation.Play();

                        if (_audioEffect != null)
                        {
                            _audioEffect.Play(_closeSound, EForceBehaviour.ForceAny);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private StaticPickup ChoosePickup(PickupData[] data)
        {
            int totalProbability = 0;

            for (int i = 0; i < data.Length; i++)
            {
                totalProbability += data[i].Probability;
            }

            if (totalProbability <= 0)
                return null;

            int targetProbability = UnityEngine.Random.Range(0, totalProbability);
            int currentProbability = 0;

            for (int i = 0; i < data.Length; i++)
            {
                currentProbability += data[i].Probability;

                if (targetProbability < currentProbability)
                {
                    return data[i].Pickup;
                }
            }

            return null;
        }

        private void OnPickupConsumed(StaticPickup pickup)
        {
            if (pickup.AutoDespawn == false)
                return;

            int index = Array.IndexOf(_nestedPickups, pickup);
            if (index >= 0)
            {
                // Clear reference, pickup will be auto despawned
                _nestedPickups[index] = null;
            }
        }

        private int GetExpirationTick(float time)
        {
            return Runner.Tick + Mathf.CeilToInt(time / Runner.DeltaTime);
        }

        // HELPERS

        private enum EBehaviour
        {
            RandomOnSpawn,
            RandomOnOpen,
        }

        [Serializable]
        private class PickupPoint
        {
            public Transform Transform;
            public PickupData[] Pickups;
        }

        [Serializable]
        private class PickupData
        {
            public StaticPickup Pickup;
            public int Probability = 1;
        }
    }
}