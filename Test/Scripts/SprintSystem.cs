using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class SprintSystem : NetworkBehaviour
    {
        private const float SPRINT_DURATION = 7.0f;
        private const float COOLDOWN_DURATION = 7.0f;
        private const float SPRINT_SPEED_MULTIPLIER = 1.5f;

        [Networked]
        public float SprintTimeRemaining { get; private set; }
        
        [Networked]
        public float CooldownTimeRemaining { get; private set; }
        
        [Networked]
        public bool IsSprinting { get; private set; }

        public bool IsOnCooldown => CooldownTimeRemaining > 0.0f;
        public bool CanSprint => !IsSprinting && !IsOnCooldown && SprintTimeRemaining <= 0.0f;
        public float SprintSpeedMultiplier => IsSprinting ? SPRINT_SPEED_MULTIPLIER : 1.0f;

        public void ProcessSprintInput(bool sprintButtonPressed)
        {
            if (HasStateAuthority == false)
                return;

            float deltaTime = Runner.DeltaTime;

            if (IsSprinting)
            {
                SprintTimeRemaining -= deltaTime;

                if (SprintTimeRemaining <= 0.0f || !sprintButtonPressed)
                {
                    StopSprint();
                }
            }
            else if (IsOnCooldown)
            {
                CooldownTimeRemaining -= deltaTime;
                
                if (CooldownTimeRemaining < 0.0f)
                {
                    CooldownTimeRemaining = 0.0f;
                }
            }
            else if (sprintButtonPressed && CanSprint)
            {
                StartSprint();
            }
        }

        private void StartSprint()
        {
            IsSprinting = true;
            SprintTimeRemaining = SPRINT_DURATION;
            CooldownTimeRemaining = 0.0f;
        }

        private void StopSprint()
        {
            IsSprinting = false;
            SprintTimeRemaining = 0.0f;
            CooldownTimeRemaining = COOLDOWN_DURATION;
        }

        public override void Spawned()
        {
            base.Spawned();
            
            SprintTimeRemaining = 0.0f;
            CooldownTimeRemaining = 0.0f;
            IsSprinting = false;
        }
    }
}
