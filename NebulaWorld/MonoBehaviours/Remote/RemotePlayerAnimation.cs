using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    // TODO: Missing client side interpolation
    public class RemotePlayerAnimation : MonoBehaviour
    {
        public PlayerAnimator PlayerAnimator;

        private float altitude;

        private void Awake()
        {
            PlayerAnimator = GetComponentInChildren<PlayerAnimator>();
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            if (PlayerAnimator == null)
            {
                return;
            }

            PlayerAnimator.jumpWeight = packet.JumpWeight;
            PlayerAnimator.jumpNormalizedTime = packet.JumpNormalizedTime;
            PlayerAnimator.idleAnimIndex = packet.IdleAnimIndex;
            PlayerAnimator.sailAnimIndex = packet.SailAnimIndex;
            PlayerAnimator.miningWeight = packet.MiningWeight;
            PlayerAnimator.miningAnimIndex = packet.MiningAnimIndex;

            PlayerAnimator.movementState = packet.MovementState;
            PlayerAnimator.horzSpeed = packet.HorzSpeed;
            PlayerAnimator.turning = packet.Turning;
            altitude = packet.Altitude;

            float deltaTime = Time.deltaTime;
            CalculateMovementStateWeights(PlayerAnimator, deltaTime);
            CalculateDirectionWeights(PlayerAnimator, deltaTime);

            PlayerAnimator.AnimateIdleState(deltaTime);
            PlayerAnimator.AnimateRunState(deltaTime);
            PlayerAnimator.AnimateDriftState(deltaTime);
            AnimateFlyState(PlayerAnimator);
            AnimateSailState(PlayerAnimator);

            PlayerAnimator.AnimateJumpState(deltaTime);
            PlayerAnimator.AnimateSkills(deltaTime);
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
            animator.flyWeight = Mathf.MoveTowards(animator.flyWeight, num, dt / ((num > 0.5) ? 0.4f : 0.2f));
            animator.sailWeight = Mathf.MoveTowards(animator.sailWeight, num2, dt / ((num2 > 0.5) ? 0.8f : 0.2f));
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
