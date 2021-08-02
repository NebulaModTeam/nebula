using NebulaModel.Attributes;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    public class PlayerAnimationUpdate
    {
        public ushort PlayerId { get; set; }

        // TODO: They don't use a finite state machine for there animation. But we need to find a way to optimized this packet.
        // maybe we could only send the variables that are used to changed the animation state.
        // See: (Game: PlayerAnimator class)
        public NebulaAnimationState Idle { get; set; }
        public NebulaAnimationState RunSlow { get; set; }
        public NebulaAnimationState RunFast { get; set; }
        public NebulaAnimationState Drift { get; set; }
        public NebulaAnimationState DriftF { get; set; }
        public NebulaAnimationState DriftL { get; set; }
        public NebulaAnimationState DriftR { get; set; }
        public NebulaAnimationState Fly { get; set; }
        public NebulaAnimationState Sail { get; set; }
        public NebulaAnimationState Mining0 { get; set; }
        // some extra values to compute backpack flame size
        // i put them here because i update the player fx together with the animation update
        public float vertSpeed { get; set; }
        public float horzSpeed { get; set; }
    }
}
