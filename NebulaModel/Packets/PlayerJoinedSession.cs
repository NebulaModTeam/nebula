namespace NebulaModel.Packets
{
    public class PlayerJoinedSession
    {
        public ushort Id { get; set; }

        public PlayerJoinedSession() { }
        public PlayerJoinedSession(ushort id) { Id = id; }
    }
}
