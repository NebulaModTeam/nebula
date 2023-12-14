namespace NebulaModel.Packets.Session;

public class PlayerDisconnected
{
    public PlayerDisconnected() { }

    public PlayerDisconnected(ushort playerId, ushort numPlayers)
    {
        PlayerId = playerId;
        NumPlayers = numPlayers;
    }

    public ushort PlayerId { get; }
    public ushort NumPlayers { get; }
}
