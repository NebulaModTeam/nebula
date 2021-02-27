using NebulaModel.DataStructures;

namespace NebulaModel.Packets
{
    public class PlayerAnimationUpdate
    {
        public ushort PlayerId { get; set; }

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
    }
}
