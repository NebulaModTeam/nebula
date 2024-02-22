namespace NebulaModel.Packets.Session;

public class GlobalGameDataResponse
{
    public GlobalGameDataResponse() { }

    public bool SandboxToolsEnabled { get; set; }
    public byte[] HistoryBinaryData { get; set; }
    public byte[] SpaceSectorBinaryData { get; set; }
    public byte[] MilestoneSystemBinaryData { get; set; }
    public byte[] TrashSystemBinaryData { get; set; }
}
