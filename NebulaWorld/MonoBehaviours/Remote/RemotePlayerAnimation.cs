using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    // TODO: Missing client side interpolation
    public class RemotePlayerAnimation : MonoBehaviour
    {
        private PlayerAnimator playerAnimator;
        private float altitude;

        private void Awake()
        {
            playerAnimator = GetComponentInChildren<PlayerAnimator>();            
            playerAnimator.Start();
            playerAnimator.enabled = false;
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            if (playerAnimator == null)
                return;

            playerAnimator.jumpWeight = packet.JumpWeight;
            playerAnimator.jumpNormalizedTime = packet.JumpNormalizedTime;
            playerAnimator.idleAnimIndex = packet.IdleAnimIndex;
            playerAnimator.sailAnimIndex = packet.SailAnimIndex;
            playerAnimator.miningWeight = packet.MiningWeight;
            playerAnimator.miningAnimIndex = packet.MiningAnimIndex;

            playerAnimator.movementState = packet.MovementState;
            playerAnimator.horzSpeed = packet.HorzSpeed;
            playerAnimator.turning = packet.Turning;
            altitude = packet.Altitude;

            float deltaTime = Time.deltaTime;
            CalculateMovementStateWeights(playerAnimator, deltaTime);
            CalculateDirectionWeights(playerAnimator, deltaTime);

            playerAnimator.AnimateIdleState(deltaTime);
            playerAnimator.AnimateRunState(deltaTime);
            playerAnimator.AnimateDriftState(deltaTime);
            AnimateFlyState(playerAnimator);
            AnimateSailState(playerAnimator);

            playerAnimator.AnimateJumpState(deltaTime);
            playerAnimator.AnimateSkills(deltaTime);
            //playerAnimator.AnimateRenderers(deltaTime);
        }

        private void CalculateMovementStateWeights(PlayerAnimator animator, float dt)
        {
            float target = (animator.horzSpeed > 0.15f) ? 1f : 0f;
            float target2 = (animator.movementState >= EMovementState.Drift) ? 1f : 0f;
            float num = (animator.movementState >= EMovementState.Fly) ? 1f : 0f;
            float num2 = (animator.movementState >= EMovementState.Sail) ? 1f : 0f;
            animator.runWeight = Mathf.MoveTowards(animator.runWeight, target, dt / 0.22f);
            animator.driftWeight = Mathf.MoveTowards(animator.driftWeight, target2, dt / 0.2f);
            animator.flyWeight = Mathf.MoveTowards(animator.flyWeight, num, dt / (((double)num > 0.5) ? 0.4f : 0.2f));
            animator.sailWeight = Mathf.MoveTowards(animator.sailWeight, num2, dt / (((double)num2 > 0.5) ? 0.8f : 0.2f));
            for (int i = 0; i < animator.sails.Length; i++)
            {
                animator.sailAnimWeights[i] = Mathf.MoveTowards(animator.sailAnimWeights[i], (i == animator.sailAnimIndex) ? 1f : 0f, dt / 0.3f);
            }
        }

        private void CalculateDirectionWeights(PlayerAnimator animator, float dt)
        {
            animator.leftWeight = Mathf.InverseLerp(-animator.minTurningAngle, -animator.maxTurningAngle, animator.turning);
            animator.rightWeight = Mathf.InverseLerp(animator.minTurningAngle, animator.maxTurningAngle, animator.turning);
            animator.forwardWeight = Mathf.Clamp01(1f - animator.leftWeight - animator.rightWeight);
            float num = Mathf.Clamp01((animator.horzSpeed - 0.01f) / ((animator.movementState == EMovementState.Drift) ? 5f : 12.5f));
            animator.forwardWeight *= num;
            animator.leftWeight *= num;
            animator.rightWeight *= num;
            animator.zeroWeight = 1f - animator.forwardWeight - animator.leftWeight - animator.rightWeight;
        }

        public void AnimateFlyState(PlayerAnimator animator)
        {
            bool flag = animator.flyWeight > 0.001f;
            animator.fly_0.enabled = flag;
            animator.fly_f.enabled = flag;
            animator.fly_l.enabled = flag;
            animator.fly_r.enabled = flag;
            animator.fly_0.weight = altitude * animator.flyWeight * animator.zeroWeight;
            animator.fly_f.weight = altitude * animator.flyWeight * animator.forwardWeight;
            animator.fly_l.weight = altitude * animator.flyWeight * animator.leftWeight;
            animator.fly_r.weight = altitude * animator.flyWeight * animator.rightWeight;
            animator.fly_0.speed = 0.44f;
            animator.fly_f.speed = 0.44f;
            animator.fly_l.speed = 0.44f;
            animator.fly_r.speed = 0.44f;
            if (!flag)
            {
                animator.fly_0.normalizedTime = 0f;
                animator.fly_f.normalizedTime = 0f;
                animator.fly_l.normalizedTime = 0f;
                animator.fly_r.normalizedTime = 0f;
            }
        }

        public void AnimateSailState(PlayerAnimator animator)
        {
            bool flag = animator.sailWeight > 0.001f;
            for (int i = 0; i < animator.sails.Length; i++)
            {
                animator.sails[i].weight = altitude * animator.sailWeight * animator.sailAnimWeights[i];
                animator.sails[i].enabled = flag;
                animator.sails[i].speed = 1f;
                if (!flag)
                {
                    animator.sails[i].normalizedTime = 0f;
                }
            }
        }
    }
}
