#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryTechRefundProcessor : PacketProcessor<GameHistoryTechRefundPacket>
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
                var itemPoints = GameMain.data.mainPlayer.mecha.lab.itemPoints;
                foreach (var item in itemPoints.items)
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
                var techProto = LDB.techs.Select(packet.TechIdContributed);
                var items = techProto.Items;
                var array = techProto.ItemPoints;

                //client should have the same research queued, seek currently needed itemIds and re-add points that were contributed
                for (var i = 0; i < array.Length; i++)
                {
                    var itemId = items[i];
                    var contributedItems = (int)packet.TechHashedContributed * array[i];
                    GameMain.data.mainPlayer.mecha.lab.itemPoints.Alter(itemId, contributedItems);
                }
                //let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();
            }
        }
    }
}
