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

    public int GeneratorId { get; }
    public int LensCount { get; }
    public int LensInc { get; }
    public int PlanetId { get; }
}
