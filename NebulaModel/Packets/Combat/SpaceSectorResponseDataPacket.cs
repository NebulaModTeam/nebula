namespace NebulaModel.Packets.Combat;

public class SpaceSectorResponseDataPacket
{
    public SpaceSectorResponseDataPacket() { }

    public SpaceSectorResponseDataPacket(byte[] spaceSectorResponseData)
    {
        SpaceSectorResponseData = spaceSectorResponseData;
    }

    public byte[] SpaceSectorResponseData { get; set; }
}
