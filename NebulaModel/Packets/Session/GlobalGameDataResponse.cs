namespace NebulaModel.Packets.Session;

public class GlobalGameDataResponse
{
    public GlobalGameDataResponse() { }

    public GlobalGameDataResponse(bool sandboxToolsEnabled,
        byte[] historyBinaryData, byte[] spaceSectorBinaryData,
        byte[] milestoneSystemBinaryData, byte[] trashSystemBinaryData)
    {
        SandboxToolsEnabled = sandboxToolsEnabled;
        HistoryBinaryData = historyBinaryData;
        SpaceSectorBinaryData = spaceSectorBinaryData;
        MilestoneSystemBinaryData = milestoneSystemBinaryData;
        TrashSystemBinaryData = trashSystemBinaryData;
    }

    public bool SandboxToolsEnabled { get; set; }
    public byte[] HistoryBinaryData { get; set; }
    public byte[] SpaceSectorBinaryData { get; set; }
    public byte[] MilestoneSystemBinaryData { get; set; }
    public byte[] TrashSystemBinaryData { get; set; }
}
