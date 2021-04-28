using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;
using NebulaModel.Logger;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryTechRefundProcessor : IPacketProcessor<GameHistoryTechRefundPacket>
    {
        public void ProcessPacket(GameHistoryTechRefundPacket packet, NebulaConnection conn)
        {
            Log.Info($"ProcessPacket: got GameHistoryTechRefundPacket");
            //only refund if we have contributed
            if(packet.TechHashedContributed > 0)
            {
                Log.Info($"ProcessPacket: contributed {packet.TechHashedContributed} hashes");
                //client should have the same research queued, seek currently needed itemIds and re-add points that were contributed
                foreach (KeyValuePair<int, int> item in GameMain.data.mainPlayer.mecha.lab.itemPoints.items)
                {
                    GameMain.data.mainPlayer.mecha.lab.itemPoints.Alter(item.Key, (int)packet.TechHashedContributed * 3600);
                    Log.Info($"ProcessPacket: added {(int)packet.TechHashedContributed * 3600} points of {item.Key} to queues");
                }
                //let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();
                Log.Info($"ProcessPacket: finished takeback");
            }
            else
            {
                Log.Info($"ProcessPacket: No refunds");
            }
        }
    }
}
