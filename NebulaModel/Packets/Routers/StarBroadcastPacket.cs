namespace NebulaModel.Packets.Routers;

public class StarBroadcastPacket
{
    public StarBroadcastPacket() { }

    public StarBroadcastPacket(byte[] packetObject, int starId)
    {
        PacketObject = packetObject;
        StarId = starId;
    }

    public byte[] PacketObject { get; }
    public int StarId { get; }
}
