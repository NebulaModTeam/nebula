#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class BeltSignalIconProcessor : PacketProcessor<BeltSignalIconPacket>
{
    protected override void ProcessPacket(BeltSignalIconPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory == null || packet.EntityId >= factory.entityCursor) return;
            factory.cargoTraffic.SetBeltSignalIcon(packet.EntityId, packet.SignalId);
        }
    }
}
