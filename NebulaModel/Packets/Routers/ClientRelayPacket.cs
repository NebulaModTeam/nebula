namespace NebulaModel.Packets.Routers;

public class ClientRelayPacket
{
    public ClientRelayPacket() { }

    public ClientRelayPacket(byte[] packetObject, string clientUsername)
    {
        PacketObject = packetObject;
        ClientUsername = clientUsername;
    }

    public byte[] PacketObject { get; set; }
    public string ClientUsername { get; set; }
}
