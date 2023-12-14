namespace NebulaModel.Packets.Factory.RayReceiver;

public class RayReceiverChangeModePacket
{
    public RayReceiverChangeModePacket() { }

    public RayReceiverChangeModePacket(int generatorId, RayReceiverMode mode, int planetId)
    {
        GeneratorId = generatorId;
        Mode = mode;
        PlanetId = planetId;
    }

    public int GeneratorId { get; }
    public RayReceiverMode Mode { get; }
    public int PlanetId { get; }
}

public enum RayReceiverMode
{
    Electricity = 0,
    Photon = 1
}
