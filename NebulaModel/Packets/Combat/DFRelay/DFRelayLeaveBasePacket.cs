namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayLeaveBasePacket
{
    public DFRelayLeaveBasePacket() { }

    public DFRelayLeaveBasePacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
        RelayNeutralizedCounter = dFRelay.hive.relayNeutralizedCounter;
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
    public int RelayNeutralizedCounter { get; set; }
}
