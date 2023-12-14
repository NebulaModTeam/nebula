namespace NebulaModel.Packets.Players;

public class PlayerUpdateLocalStarId
{
    public PlayerUpdateLocalStarId() { }

    public PlayerUpdateLocalStarId(int starId)
    {
        StarId = starId;
    }

    public int StarId { get; }
}
