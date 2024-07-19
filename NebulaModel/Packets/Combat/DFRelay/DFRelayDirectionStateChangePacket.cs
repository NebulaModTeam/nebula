namespace NebulaModel.Packets.Combat.DFRelay
{
    public class DFRelayDirectionStateChangePacket
    {
        public DFRelayDirectionStateChangePacket() { }

        public DFRelayDirectionStateChangePacket(in int relayId, in int hiveAstroId, in int stage, in int newDirection)
        {
            HiveAstroId = hiveAstroId;
            RelayId = relayId;
            Stage = stage;
            NewDirection = newDirection;
        }

        public int HiveAstroId { get; set; }
        public int RelayId { get; set; }
        public int Stage { get; set; }
        public int NewDirection { get; set; }
    }
}
