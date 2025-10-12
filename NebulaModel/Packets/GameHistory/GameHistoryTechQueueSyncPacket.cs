using System;

namespace NebulaModel.Packets.GameHistory;

public class GameHistoryTechQueueSyncPacket
{
    public GameHistoryTechQueueSyncPacket() { }

    public GameHistoryTechQueueSyncPacket(bool isRequest)
    {
        IsRequest = isRequest;
        TechQueue = Array.Empty<int>();
    }

    public GameHistoryTechQueueSyncPacket(int[] techQueue)
    {
        IsRequest = false;
        TechQueue = techQueue;
    }

    public int[] TechQueue { get; set; }
    public bool IsRequest { get; set; }
}
