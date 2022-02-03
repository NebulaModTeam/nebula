using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    // TODO: Missing client side interpolation
    public class RemotePlayerAnimation : MonoBehaviour
    {
        public PlayerAnimator PlayerAnimator;
        private RemotePlayerMovement rootMovement;
        private RemotePlayerEffects remotePlayerEffects;
        private float altitudeFactor;
        private readonly PlayerAnimationUpdate[] packetBuffer = new PlayerAnimationUpdate[3];

        private void Awake()
        {
            PlayerAnimator = GetComponent<PlayerAnimator>();
            rootMovement = GetComponent<RemotePlayerMovement>();
            remotePlayerEffects = GetComponent<RemotePlayerEffects>();
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            // Delay for 200 ms
            for (int i = 0; i < packetBuffer.Length - 1; ++i)
            {
                packetBuffer[i] = packetBuffer[i + 1];
            }
            packetBuffer[packetBuffer.Length - 1] = packet;
            packet = packetBuffer[0];
            if (packet == null || PlayerAnimator == null)
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
            PlanetData localPlanet = GameMain.galaxy.PlanetById(rootMovement.localPlanetId);
            altitudeFactor = (localPlanet == null) ? 1f : Mathf.Clamp01((transform.position.magnitude - localPlanet.realRadius - 7f) * 0.15f);

            float deltaTime = Time.deltaTime;
            CalculateMovementStateWeights(PlayerAnimator, deltaTime);
            CalculateDirectionWeights(PlayerAnimator);

            PlayerAnimator.AnimateIdleState(deltaTime);
            if (PlayerAnimator.idleAnimIndex == 0)
            {
                for (int i = 1; i < PlayerAnimator.idles.Length; i++)
                {
                    PlayerAnimator.idles[i].weight = 0;
                    PlayerAnimator.idles[i].normalizedTime = 0f;
                }
            }
            PlayerAnimator.AnimateRunState(deltaTime);
            PlayerAnimator.AnimateDriftState(deltaTime);
            AnimateFlyState(PlayerAnimator);
            AnimateSailState(PlayerAnimator);

            PlayerAnimator.AnimateJumpState(deltaTime);
            PlayerAnimator.AnimateSkills(deltaTime);
            AnimateRenderers(PlayerAnimator);

            remotePlayerEffects.UpdateState(packet);
        }

        private void CalculateMovementStateWeights(PlayerAnimator animator, float dt)
        {
            float runTarget = (animator.horzSpeed > 0.15f) ? 1f : 0f;
            float driftTarget = (animator.movementState >= EMovementState.Drift) ? 1f : 0f;
            float flyTarget = (animator.movementState >= EMovementState.Fly) ? 1f : 0f;
            float sailTarget = (animator.movementState >= EMovementState.Sail) ? 1f : 0f;
            animator.runWeight = Mathf.MoveTowards(animator.runWeight, runTarget, dt / 0.22f);
            animator.driftWeight = Mathf.MoveTowards(animator.driftWeight, driftTarget, dt / 0.2f);
            animator.flyWeight = Mathf.MoveTowards(animator.flyWeight, flyTarget, dt / ((flyTarget > 0.5) ? 0.4f : 0.2f));
            animator.sailWeight = Mathf.MoveTowards(animator.sailWeight, sailTarget, dt / ((sailTarget > 0.5) ? 0.8f : 0.2f));
            for (int i = 0; i < animator.sails.Length; i++)
            {
                animator.sailAnimWeights[i] = Mathf.MoveTowards(animator.sailAnimWeights[i], (i == animator.sailAnimIndex) ? 1f : 0f, dt / 0.3f);
            }
        }

        private void CalculateDirectionWeights(PlayerAnimator animator)
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
            animator.fly_0.weight = altitudeFactor * animator.flyWeight * animator.zeroWeight;
            animator.fly_f.weight = altitudeFactor * animator.flyWeight * animator.forwardWeight;
            animator.fly_l.weight = altitudeFactor * animator.flyWeight * animator.leftWeight;
            animator.fly_r.weight = altitudeFactor * animator.flyWeight * animator.rightWeight;
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
                animator.sails[i].weight = altitudeFactor * animator.sailWeight * animator.sailAnimWeights[i];
                animator.sails[i].enabled = flag;
                animator.sails[i].speed = 1f;
                if (!flag)
                {
                    animator.sails[i].normalizedTime = 0f;
                }
            }
        }

        public void AnimateRenderers(PlayerAnimator animator)
        {
            //animator.player.mechaArmorModel.inst_armor_mat.SetVector("_InitPositionSet", transform.position);
            animator.player.mechaArmorModel.inst_part_ar_mat.SetVector("_InitPositionSet", transform.position);
            animator.player.mechaArmorModel.inst_part_sk_mat.SetVector("_InitPositionSet", transform.position);
            for (int i = 0; i < 8; i++)
            {
                animator.player.mechaArmorModel.bone_mats_inst[i].SetVector("_InitPositionSet", transform.position);
            }
        }

        private void LateUpdate()
        {
            // fixes weird player movement
            PlayerAnimator.bipBone.localPosition -= PlayerAnimator.motorBone.localPosition;
        }
    }
}
