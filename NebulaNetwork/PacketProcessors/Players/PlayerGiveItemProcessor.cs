#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerGiveItemProcessor : PacketProcessor<PlayerGiveItemPacket>
{
    protected override void ProcessPacket(PlayerGiveItemPacket packet, NebulaConnection conn)
    {
        GameMain.mainPlayer.TryAddItemToPackage(packet.ItemId, packet.ItemCount, packet.ItemInc, true);
    }
}
