#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryTechQueueSyncProcessor : PacketProcessor<GameHistoryTechQueueSyncPacket>
{
    protected override void ProcessPacket(GameHistoryTechQueueSyncPacket packet, NebulaConnection conn)
    {
        if (packet.IsRequest)
        {
            packet.IsRequest = false;
            packet.TechQueue = GameMain.history.techQueue;
            conn.SendPacket(packet);
        }
        else
        {
            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                var length = GameMain.history.techQueue.Length;
                for (var i = 0; i < length; i++)
                {
                    // Clear the original queue by dequeue for compatibility
                    GameMain.history.DequeueTech();
                }
                for (var i = 0; i < packet.TechQueue.Length; i++)
                {
                    if (packet.TechQueue[i] == 0) return;
                    GameMain.history.EnqueueTech(packet.TechQueue[i]);
                }
            }
            if (IsHost)
            {
                // Broadcast to other players
                Multiplayer.Session.Network.SendPacketExclude(packet, conn);
            }
        }
    }
}
