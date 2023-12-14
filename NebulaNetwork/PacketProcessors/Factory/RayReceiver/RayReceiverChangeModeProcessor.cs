#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.RayReceiver;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.RayReceiver;

[RegisterPacketProcessor]
internal class RayReceiverChangeModeProcessor : PacketProcessor<RayReceiverChangeModePacket>
{
    protected override void ProcessPacket(RayReceiverChangeModePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem.genPool;
        if (pool == null || packet.GeneratorId == -1 || packet.GeneratorId >= pool.Length || pool[packet.GeneratorId].id == -1)
        {
            return;
        }
        switch (packet.Mode)
        {
            case RayReceiverMode.Electricity:
                pool[packet.GeneratorId].productId = 0;
                pool[packet.GeneratorId].productCount = 0f;
                break;
            case RayReceiverMode.Photon:
                {
                    var protoId = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory
                        ?.entityPool[pool[packet.GeneratorId].entityId].protoId;
                    if (protoId != null)
                    {
                        var itemProto = LDB.items.Select((int)protoId);
                        pool[packet.GeneratorId].productId = itemProto.prefabDesc.powerProductId;
                    }
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Invalid RayReceiverMode type: " + packet.Mode);
        }
    }
}
