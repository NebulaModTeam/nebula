namespace NebulaModel.Packets.Routers;

public class PlanetBroadcastPacket
{
    public PlanetBroadcastPacket() { }

    public PlanetBroadcastPacket(byte[] packetObject, int planetId)
    {
        PacketObject = packetObject;
        PlanetId = planetId;
    }

    public byte[] PacketObject { get; set; }
    public int PlanetId { get; set; }
}
