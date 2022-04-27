namespace NebulaModel.Packets.Statistics
{
    public class MilestoneUnlockPacket
    {
        public int Id { get; set; }
        public long UnlockTick { get; set; }
        public int PatternId { get; set; }
        public long[] Parameters { get; set; }

        public MilestoneUnlockPacket() { }
        public MilestoneUnlockPacket(int id, long unlockTick, int patternId, long[] parameters)
        {
            Id = id;
            UnlockTick = unlockTick;
            PatternId = patternId;
            Parameters = parameters;
        }
    }
}
