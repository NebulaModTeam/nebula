namespace NebulaModel.Packets.Warning;

public class WarningDataPacket
{
    public int ActiveWarningCount { get; set; }
    public int Tick { get; set; }
    public byte[] BinaryData { get; set; }
}
