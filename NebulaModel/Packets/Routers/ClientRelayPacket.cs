namespace NebulaModel.Packets.Routers;

public class ClientRelayPacket
{
    public ClientRelayPacket() { }

    public ClientRelayPacket(byte[] packetObject, ushort clientUserId)
    {
        PacketObject = packetObject;
        ClientUserId = clientUserId;
    }

    public byte[] PacketObject { get; set; }
    public ushort ClientUserId { get; set; }
}
