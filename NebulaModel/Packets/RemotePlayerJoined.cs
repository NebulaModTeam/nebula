namespace NebulaModel.Packets
{
    public class RemotePlayerJoined
    {
        public ushort PlayerId { get; set; }

        public RemotePlayerJoined() { }
        public RemotePlayerJoined(ushort id) { PlayerId = id; }
    }
}
