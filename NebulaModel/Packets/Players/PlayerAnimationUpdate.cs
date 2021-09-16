using NebulaAPI;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    public class PlayerAnimationUpdate
    {
        public ushort PlayerId { get; set; }
        public float RunWeight { get; set; }
        public float DriftWeight { get; set; }
        public float FlyWeight { get; set; }
        public float SailWeight { get; set; }
        public float JumpWeight { get; set; }
        public float JumpNormalizedTime { get; set; }
        public int IdleAnimIndex { get; set; }
        public int SailAnimIndex { get; set; }
        public float MiningWeight { get; set; }
        public int MiningAnimIndex { get; set; }
        public float[] SailAnimWeights { get; set; }

        public PlayerAnimationUpdate() { }

        public PlayerAnimationUpdate(ushort playerId, PlayerAnimator animator)
        {
            PlayerId = playerId;

            RunWeight = animator.runWeight;
            DriftWeight = animator.driftWeight;
            FlyWeight = animator.flyWeight;
            SailWeight = animator.sailWeight;
            JumpWeight = animator.jumpWeight;
            JumpNormalizedTime = animator.jumpNormalizedTime;
            IdleAnimIndex = animator.idleAnimIndex;
            SailAnimIndex = animator.sailAnimIndex;
            MiningWeight = animator.miningWeight;
            MiningAnimIndex = animator.miningAnimIndex;
            SailAnimWeights = animator.sailAnimWeights;
        }
    }
}
