namespace NebulaModel.Packets.Session
{
    public class PlayerDisconnected
    {
        public ushort PlayerId { get; set; }
        public ushort NumPlayers { get; set; }

        public PlayerDisconnected() { }
        public PlayerDisconnected(ushort playerId, ushort numPlayers)
        {
            PlayerId = playerId;
            NumPlayers = numPlayers;
        }
    }
}
