namespace NebulaModel.Packets.Statistics
{
    public class MilestoneDataResponse
    {
        public byte[] BinaryData { get; set; }

        public MilestoneDataResponse() { }
        public MilestoneDataResponse(byte[] binaryData)
        {
            BinaryData = binaryData;
        }
    }
}
