#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
{
    protected override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
    {
        var objPool = GameMain.data.trashSystem.container.trashObjPool;
        if (packet.TrashId < 0 || packet.TrashId >= objPool.Length) return;

        ref var trashObj = ref objPool[packet.TrashId];

        if (IsHost)
        {
            if (trashObj.item == 0 && trashObj.count == 0) return; //Already delete
            //Approve and broadcast the remove event
            GameMain.data.trashSystem.RemoveTrash(packet.TrashId);
        }
        else
        {
            if (trashObj.count == 0) return; //Empty
            //Revert itemId back before removing
            trashObj.item = packet.ItemId;
            using (Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers.On())
            {
                GameMain.data.trashSystem.RemoveTrash(packet.TrashId);
            }
            Multiplayer.Session.Trashes.ClientTrashCount--;
            if (Multiplayer.Session.Trashes.ClientTrashCount <= 0)
                Multiplayer.Session.Trashes.ClientTrashCount = 0;
        }
    }
}
