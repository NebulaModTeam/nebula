namespace NebulaModel.Packets.Session
{
    public class PlayerDisconnected
    {
        public ushort PlayerId { get; set; }

        public PlayerDisconnected() { }
        public PlayerDisconnected(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
