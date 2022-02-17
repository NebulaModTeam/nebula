namespace NebulaModel.Packets.Players
{
    public class PlayerSandCount
    {
        public int SandCount { get; set; }
        public PlayerSandCount() { }
        public PlayerSandCount(int sandCount)
        {
            SandCount = sandCount;
        }
    }
}
