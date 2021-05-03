using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryTechRefundProcessor : IPacketProcessor<GameHistoryTechRefundPacket>
    {
        public void ProcessPacket(GameHistoryTechRefundPacket packet, NebulaConnection conn)
        {
            //only refund if we have contributed
            if(packet.TechHashedContributed > 0)
            {
                //client should have the same research queued, seek currently needed itemIds and re-add points that were contributed
                ItemPack itemPoints = GameMain.data.mainPlayer.mecha.lab.itemPoints;
                foreach (KeyValuePair<int, int> item in itemPoints.items)
                {
                    itemPoints.Alter(item.Key, (int)packet.TechHashedContributed * 3600);
                }
                //let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();
            }
        }
    }
}
