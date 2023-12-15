namespace NebulaModel.Packets.GameHistory;

public class GameHistoryFeatureKeyPacket
{
    public GameHistoryFeatureKeyPacket() { }

    public GameHistoryFeatureKeyPacket(int featureId, bool add)
    {
        FeatureId = featureId;
        Add = add;
    }

    public int FeatureId { get; set; }
    public bool Add { get; set; }
}
