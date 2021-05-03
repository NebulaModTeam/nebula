using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;
using NebulaModel.Logger;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryTechRefundProcessor : IPacketProcessor<GameHistoryTechRefundPacket>
    {
        public void ProcessPacket(GameHistoryTechRefundPacket packet, NebulaConnection conn)
        {
            //only refund if we have contributed
            if(packet.TechHashedContributed > 0)
            {
                Log.Info($"ProcessPacket: contributed {packet.TechHashedContributed} hashes to itemid {packet.TechIdContributed}");
                TechProto techProto = LDB.techs.Select(packet.TechIdContributed);
                int[] items = techProto.Items;
                int[] array = techProto.ItemPoints;

                //client should have the same research queued, seek currently needed itemIds and re-add points that were contributed
                for (int i=0; i < array.Length; i++)
                {
                    int itemId = items[i];
                    int contributedItems = (int)packet.TechHashedContributed * array[i];
                    GameMain.data.mainPlayer.mecha.lab.itemPoints.Alter(itemId, contributedItems);
                }
                //let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();
            }
        }
    }
}
