namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayArriveBasePacket
{
    public DFRelayArriveBasePacket() { }

    public DFRelayArriveBasePacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
        HiveRtseed = dFRelay.hive.rtseed;
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
    public int HiveRtseed { get; set; }
}
