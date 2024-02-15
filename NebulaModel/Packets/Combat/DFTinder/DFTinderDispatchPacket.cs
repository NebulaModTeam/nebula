namespace NebulaModel.Packets.Combat.DFTinder;

public class DFTinderDispatchPacket
{
    public DFTinderDispatchPacket() { }

    public DFTinderDispatchPacket(in DFTinderComponent dFTinder)
    {
        OriginHiveAstroId = dFTinder.originHiveAstroId;
        TargetHiveAstroId = dFTinder.targetHiveAstroId;
        TinderId = dFTinder.id;
    }

    public int OriginHiveAstroId { get; set; }
    public int TargetHiveAstroId { get; set; }
    public int TinderId { get; set; }
}
