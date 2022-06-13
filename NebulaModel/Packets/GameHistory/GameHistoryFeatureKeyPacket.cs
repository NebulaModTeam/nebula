namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryFeatureKeyPacket
    {
        public int FeatureId { get; set; }
        public bool Add { get; set; }

        public GameHistoryFeatureKeyPacket() { }
        public GameHistoryFeatureKeyPacket(int featureId, bool add)
        {
            FeatureId = featureId;
            Add = add;
        }
    }
}
