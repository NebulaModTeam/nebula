using NebulaAPI.DataStructures;

namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayLeaveDockPacket
{
    public DFRelayLeaveDockPacket() { }

    public DFRelayLeaveDockPacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
        TargetAstroId = dFRelay.targetAstroId;
        BaseId = dFRelay.baseId;
        TargetLPos = dFRelay.targetLPos.ToFloat3();
        TargetYaw = dFRelay.targetYaw;
        BaseState = dFRelay.baseState;
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
    public int TargetAstroId { get; set; }
    public int BaseId { get; set; }
    public Float3 TargetLPos { get; set; }
    public float TargetYaw { get; set; }
    public float BaseState { get; set; }
}
