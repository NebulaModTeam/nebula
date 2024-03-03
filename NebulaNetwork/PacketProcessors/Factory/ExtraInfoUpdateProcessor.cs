#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
internal class ExtraInfoUpdateProcessor : PacketProcessor<ExtraInfoUpdatePacket>
{
    protected override void ProcessPacket(ExtraInfoUpdatePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }

        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            if (packet.ObjId < 0)
            {
                var prebuildId = -packet.ObjId;
                if (prebuildId > 0 && prebuildId < factory.prebuildCursor)
                {
                    factory.WriteExtraInfoOnPrebuild(prebuildId, packet.Info);
                }
            }
            else
            {
                var entityId = packet.ObjId;
                if (entityId > 0 && entityId < factory.entityCursor)
                {
                    factory.WriteExtraInfoOnEntity(entityId, packet.Info);
                }
            }
        }
    }
}
