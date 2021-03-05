namespace NebulaModel.Packets.Session
{
    public class PlayerJoining
    {
        public ushort PlayerId { get; set; }

        public PlayerJoining() { }
        public PlayerJoining(ushort id) { PlayerId = id; }
    }
}
