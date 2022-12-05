namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryDataResponse
    {
        public byte[] HistoryBinaryData { get; set; }
        public bool SandboxToolsEnabled { get; set; }

        public GameHistoryDataResponse() { }
        public GameHistoryDataResponse(byte[] historyBinaryData, bool sandboxToolsEnabled)
        {
            HistoryBinaryData = historyBinaryData;
            SandboxToolsEnabled = sandboxToolsEnabled;
        }
    }
}
