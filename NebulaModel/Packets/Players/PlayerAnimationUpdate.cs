using NebulaAPI;
using NebulaModel.DataStructures;
using Unity;
using UnityEngine;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    public class PlayerAnimationUpdate
    {
        public ushort PlayerId { get; set; }
        public float JumpWeight { get; set; }
        public float JumpNormalizedTime { get; set; }
        public int IdleAnimIndex { get; set; }
        public int SailAnimIndex { get; set; }
        public float MiningWeight { get; set; }
        public int MiningAnimIndex { get; set; }

        public EMovementState MovementState { get; set; }
        public float HorzSpeed { get; set; }
        public float Turning { get; set; }
        public float Altitude { get; set; }

        public PlayerAnimationUpdate() { }

        public PlayerAnimationUpdate(ushort playerId, PlayerAnimator animator)
        {
            PlayerId = playerId;

            JumpWeight = animator.jumpWeight;
            JumpNormalizedTime = animator.jumpNormalizedTime;
            IdleAnimIndex = animator.idleAnimIndex;
            SailAnimIndex = animator.sailAnimIndex;
            MiningWeight = animator.miningWeight;
            MiningAnimIndex = animator.miningAnimIndex;

            MovementState = animator.movementState;
            HorzSpeed = animator.horzSpeed;
            Turning = animator.turning;
            Altitude = 1f;
            if (GameMain.localPlanet != null)
            {
                Altitude = Mathf.Clamp01((animator.player.position.magnitude - GameMain.localPlanet.realRadius - 7f) * 0.15f);
            }
        }
    }
}
