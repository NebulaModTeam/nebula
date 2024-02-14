namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayArriveDockPacket
{
    public DFRelayArriveDockPacket() { }

    public DFRelayArriveDockPacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
}
