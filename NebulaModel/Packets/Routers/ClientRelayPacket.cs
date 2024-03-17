namespace NebulaModel.Packets.Routers;

public class ClientRelayPacket
{
    public ClientRelayPacket() { }

    public ClientRelayPacket(byte[] packetObject, ushort clientUserId)
    {
        // TODO: We should probably rename this to PacketObjectToRelay or something
        PacketObject = packetObject;
        // TODO: We should probably rename this to RecipientClientUserId or something
        ClientUserId = clientUserId;
    }

    public byte[] PacketObject { get; set; }
    public ushort ClientUserId { get; set; }
}
