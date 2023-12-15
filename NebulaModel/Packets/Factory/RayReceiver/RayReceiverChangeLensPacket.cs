namespace NebulaModel.Packets.Factory.RayReceiver;

public class RayReceiverChangeLensPacket
{
    public RayReceiverChangeLensPacket() { }

    public RayReceiverChangeLensPacket(int generatorId, int lensCount, int lensInc, int planetId)
    {
        GeneratorId = generatorId;
        LensCount = lensCount;
        LensInc = lensInc;
        PlanetId = planetId;
    }

    public int GeneratorId { get; set; }
    public int LensCount { get; set; }
    public int LensInc { get; set; }
    public int PlanetId { get; set; }
}
