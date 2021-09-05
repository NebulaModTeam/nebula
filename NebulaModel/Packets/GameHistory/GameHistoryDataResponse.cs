namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryDataResponse
    {
        public byte[] HistoryBinaryData { get; set; }

        public GameHistoryDataResponse() { }
        public GameHistoryDataResponse(byte[] historyBinaryData)
        {
            HistoryBinaryData = historyBinaryData;
        }
    }
}
