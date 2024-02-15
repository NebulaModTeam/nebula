namespace NebulaModel.Packets.Players;

public class PlayerUpdateLocalStarId
{
    public PlayerUpdateLocalStarId() { }

    public PlayerUpdateLocalStarId(ushort playerId, int starId)
    {
        PlayerId = playerId;
        StarId = starId;
    }

    public ushort PlayerId { get; set; }
    public int StarId { get; set; }
}
