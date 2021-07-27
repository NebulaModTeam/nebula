using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryTechRefundProcessor : PacketProcessor<GameHistoryTechRefundPacket>
    {
        public override void ProcessPacket(GameHistoryTechRefundPacket packet, NebulaConnection conn)
        {
            // TODO: TRY TO MERGE THESE BETTER

            if (IsHost)
            {
                //only refund if we have contributed
                if (packet.TechHashedContributed > 0)
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
            else
            {
                //only refund if we have contributed
                if (packet.TechHashedContributed > 0)
                {
                    TechProto techProto = LDB.techs.Select(packet.TechIdContributed);
                    int[] items = techProto.Items;
                    int[] array = techProto.ItemPoints;

                    //client should have the same research queued, seek currently needed itemIds and re-add points that were contributed
                    for (int i = 0; i < array.Length; i++)
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
}
