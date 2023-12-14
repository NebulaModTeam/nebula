namespace NebulaModel.Packets.GameHistory;

public class GameHistoryDataResponse
{
    public GameHistoryDataResponse() { }

    public GameHistoryDataResponse(byte[] historyBinaryData, bool sandboxToolsEnabled)
    {
        HistoryBinaryData = historyBinaryData;
        SandboxToolsEnabled = sandboxToolsEnabled;
    }

    public byte[] HistoryBinaryData { get; }
    public bool SandboxToolsEnabled { get; }
}
