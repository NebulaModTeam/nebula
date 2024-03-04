namespace NebulaModel.Packets.GameHistory;

public class GameHistoryTechQueueSyncRequest
{
    public GameHistoryTechQueueSyncRequest() { }

    public GameHistoryTechQueueSyncRequest(int[] techQueue)
    {
        TechQueue = techQueue;
    }

    public int[] TechQueue { get; set; }
}
