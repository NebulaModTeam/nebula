namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGUpdateBaseStatusPacket
{
    public DFGUpdateBaseStatusPacket() { }

    public DFGUpdateBaseStatusPacket(in DFGBaseComponent dFGBase)
    {
        PlanetId = dFGBase.groundSystem.planet.id;
        BaseId = dFGBase.id;
        ref var evolveData = ref dFGBase.evolve;
        Threat = evolveData.threat;
        Level = evolveData.level;
        Expl = evolveData.expl;
        Expf = evolveData.expf;
    }

    public void Record(in DFGBaseComponent dFGBase)
    {
        ref var evolveData = ref dFGBase.evolve;
        Threat = evolveData.threat;
        Level = evolveData.level;
        Expl = evolveData.expl;
        Expf = evolveData.expf;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public int Threat { get; set; }
    public int Level { get; set; }
    public int Expl { get; set; }
    public int Expf { get; set; }
}
