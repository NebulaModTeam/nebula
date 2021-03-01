namespace NebulaClient.GameLogic
{
    public class Player
    {
        public ushort PlayerId { get; protected set; }

        public Player(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
