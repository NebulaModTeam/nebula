namespace NebulaModel.Packets.Routers;

public class ClientRelayFailurePacket
{
    public ClientRelayFailurePacket() { }

    public ClientRelayFailurePacket(byte[] packetObject, ushort clientUserId)
    {
        // TODO: We should probably rename this to PacketObjectThatFailedToBeRelayed or something
        PacketObject = packetObject;
        // TODO: We should probably rename this to RecipientClientUserId or something
        ClientUserId = clientUserId;
    }

    public byte[] PacketObject { get; set; }
    public ushort ClientUserId { get; set; }
}
