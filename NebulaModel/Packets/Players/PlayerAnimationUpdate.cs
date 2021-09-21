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
        public byte IdleAnimIndex { get; set; }
        public byte SailAnimIndex { get; set; }        
        public byte MiningAnimIndex { get; set; }
        public float MiningWeight { get; set; }

        public EMovementState MovementState { get; set; }
        public float HorzSpeed { get; set; }
        public float VertSpeed { get; set; }
        public float Turning { get; set; }
        public EFlags Flags { get; set; }

        public enum EFlags : byte
        {
            isGrounded = 1,
            inWater = 2,
        }

        public PlayerAnimationUpdate() { }

        public PlayerAnimationUpdate(ushort playerId, PlayerAnimator animator)
        {
            PlayerId = playerId;

            JumpWeight = animator.jumpWeight;
            JumpNormalizedTime = animator.jumpNormalizedTime;
            //Compress AnimIndex, assume their values are less than 256
            IdleAnimIndex = (byte)animator.idleAnimIndex;
            SailAnimIndex = (byte)animator.sailAnimIndex;
            MiningAnimIndex = (byte)animator.miningAnimIndex;
            MiningWeight = animator.miningWeight;            

            MovementState = animator.movementState;
            HorzSpeed = animator.controller.horzSpeed;
            VertSpeed = animator.controller.vertSpeed;
            Turning = animator.turning;

            Flags = 0;
            if (animator.controller.actionWalk.isGrounded)
                Flags |= EFlags.isGrounded;
            if (animator.controller.actionDrift.inWater)
                Flags |= EFlags.inWater;
        }
    }
}
