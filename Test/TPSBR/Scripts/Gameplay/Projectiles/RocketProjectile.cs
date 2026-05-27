using UnityEngine;
using Fusion;
using System.Collections.Generic;

namespace TPSBR
{
    public class RocketProjectile : KinematicProjectile
    {
        [Header("Rocket Settings")]
        [SerializeField]
        private float _explosionRadius = 5f;
        [SerializeField]
        private float _explosionDamage = 100f;
        [SerializeField]
        private GameObject _explosionEffect;
        [SerializeField]
        private LayerMask _explosionHitMask;
        [SerializeField]
        private float _rocketSpeed = 30f;
        [SerializeField]
        private bool _useGravity = false;
        [SerializeField]
        private float _armingDelay = 0.2f;
        
        [Header("Self Damage")]
        [SerializeField]
        private bool _canDamageSelf = true;
        [SerializeField]
        private float _selfDamageMultiplier = 0.5f;

        private const int MAX_EXPLOSION_TARGETS = 32;
        private Collider[] _hitColliders = new Collider[MAX_EXPLOSION_TARGETS];
        private float _spawnTime;
        private NetworkObject _ownerObject;
        private Collider _rocketCollider;
        private Rigidbody _rigidbody;
        private Vector3 _initialVelocity;

        public override void Fire(NetworkObject owner, Vector3 firePosition, Vector3 initialVelocity, LayerMask hitMask, EHitType hitType)
        {
            if (_useGravity == false)
            {
                initialVelocity = initialVelocity.normalized * _rocketSpeed;
            }

            _explosionHitMask = hitMask;
            _ownerObject = owner;
            _initialVelocity = initialVelocity;
            
            // Call base.Fire to set up networked data, but we'll override movement
            base.Fire(owner, firePosition, initialVelocity, hitMask, hitType);
        }

        public override void Spawned()
        {
            base.Spawned();
            _spawnTime = Time.time;
            _rocketCollider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            
            // Apply initial velocity to rigidbody for physics-based movement
            if (_rigidbody != null && _initialVelocity != Vector3.zero)
            {
                _rigidbody.linearVelocity = _initialVelocity;
                Debug.Log($"[RocketProjectile] Spawned with velocity: {_initialVelocity.magnitude} m/s");
            }
            
            if (_ownerObject != null && _armingDelay > 0f)
            {
                var ownerColliders = _ownerObject.GetComponentsInChildren<Collider>();
                foreach (var ownerCollider in ownerColliders)
                {
                    if (_rocketCollider != null && ownerCollider != null)
                    {
                        Physics.IgnoreCollision(_rocketCollider, ownerCollider, true);
                    }
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Call base to handle network sync, but we override the movement
            base.FixedUpdateNetwork();
            
            // Apply physics-based movement via rigidbody
            if (_rigidbody != null && !_rigidbody.isKinematic && HasStateAuthority)
            {
                // Keep the rocket moving at constant speed
                Vector3 velocity = _rigidbody.linearVelocity;
                if (velocity.magnitude > 0.1f && velocity.magnitude < _rocketSpeed * 0.9f)
                {
                    // Re-apply velocity if it's slowed down
                    _rigidbody.linearVelocity = velocity.normalized * _rocketSpeed;
                }
            }
        }

        protected void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[RocketProjectile] OnCollisionEnter with {collision.gameObject.name}, HasStateAuthority: {HasStateAuthority}");
            
            if (HasStateAuthority == false)
                return;

            if (Time.time - _spawnTime < _armingDelay)
            {
                Debug.Log($"[RocketProjectile] Too early to explode. Time since spawn: {Time.time - _spawnTime}, Arming delay: {_armingDelay}");
                return;
            }

            Debug.Log($"[RocketProjectile] Exploding at {collision.contacts[0].point}");
            Explode(collision.contacts[0].point, collision.contacts[0].normal);
        }

        private void Explode(Vector3 explosionPosition, Vector3 normal)
        {
            if (HasStateAuthority == false)
                return;

            Debug.Log($"[RocketProjectile] Explode called. Radius: {_explosionRadius}, Damage: {_explosionDamage}, HitMask: {_explosionHitMask.value}");
            
            SpawnExplosionEffect(explosionPosition, normal);

            int hitCount = Physics.OverlapSphereNonAlloc(
                explosionPosition,
                _explosionRadius,
                _hitColliders,
                _explosionHitMask
            );
            
            Debug.Log($"[RocketProjectile] Found {hitCount} colliders in explosion radius. HitMask value: {_explosionHitMask.value}");

            var hitTargets = new HashSet<IHitTarget>();
            var player = Context != null && Context.NetworkGame != null ? Context.NetworkGame.GetPlayer(Object.InputAuthority) : null;
            var owner = player != null ? player.ActiveAgent : null;
            
            Debug.Log($"[RocketProjectile] Owner: {(owner != null ? owner.gameObject.name : "null")}");

            for (int i = 0; i < hitCount; i++)
            {
                var hitCollider = _hitColliders[i];
                if (hitCollider == null)
                    continue;

                Debug.Log($"[RocketProjectile] Checking collider {i}: {hitCollider.gameObject.name}, Layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)}");

                var hitTarget = hitCollider.GetComponentInParent<IHitTarget>();
                if (hitTarget == null)
                {
                    Debug.Log($"[RocketProjectile] No IHitTarget found on {hitCollider.gameObject.name}");
                    continue;
                }

                if (hitTargets.Contains(hitTarget))
                    continue;

                var hitAgent = hitCollider.GetComponentInParent<Agent>();
                bool isSelf = owner != null && hitAgent == owner;
                
                Debug.Log($"[RocketProjectile] HitAgent: {(hitAgent != null ? hitAgent.gameObject.name : "null")}, IsSelf: {isSelf}");

                hitTargets.Add(hitTarget);

                float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / _explosionRadius);
                damageMultiplier = Mathf.Clamp01(damageMultiplier);

                float damage = _explosionDamage * damageMultiplier;
                
                if (isSelf && _canDamageSelf)
                {
                    damage *= _selfDamageMultiplier;
                }
                else if (isSelf && !_canDamageSelf)
                {
                    continue;
                }

                if (damage > 0f)
                {
                    Vector3 direction = (hitCollider.transform.position - explosionPosition).normalized;
                    
                    HitData hitData = new HitData
                    {
                        Action = EHitAction.Damage,
                        Amount = damage,
                        Position = hitCollider.transform.position,
                        Normal = -direction,
                        Direction = direction,
                        InstigatorRef = Object.InputAuthority,
                        Target = hitTarget,
                        HitType = EHitType.RocketLauncher
                    };

                    Debug.Log($"[RocketProjectile] Applying {damage} damage to {hitCollider.gameObject.name} (isSelf: {isSelf})");
                    hitTarget.ProcessHit(ref hitData);
                }
                else
                {
                    Debug.Log($"[RocketProjectile] Damage is 0, skipping {hitCollider.gameObject.name}");
                }
            }

            Runner.Despawn(Object);
        }

        private void SpawnExplosionEffect(Vector3 position, Vector3 normal)
        {
            if (_explosionEffect == null)
                return;

            var networkBehaviour = _explosionEffect.GetComponent<NetworkBehaviour>();
            if (networkBehaviour != null)
            {
                Runner.Spawn(networkBehaviour, position, Quaternion.LookRotation(normal), Object.InputAuthority);
            }
            else
            {
                var effect = Context.ObjectCache.Get(_explosionEffect);
                effect.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
            }
        }
    }
}
