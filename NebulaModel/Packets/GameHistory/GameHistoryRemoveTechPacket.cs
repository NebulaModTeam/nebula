namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryRemoveTechPacket
    {
        public int Index { get; set; }

        public GameHistoryRemoveTechPacket() { }

        public GameHistoryRemoveTechPacket(int index)
        {
            this.Index = index;
        }
    }
}