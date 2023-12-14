namespace NebulaModel.Packets.Statistics;

public class MilestoneDataResponse
{
    public MilestoneDataResponse() { }

    public MilestoneDataResponse(byte[] binaryData)
    {
        BinaryData = binaryData;
    }

    public byte[] BinaryData { get; }
}
