#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class DispenserCourierProcessor : PacketProcessor<DispenserCourierPacket>
{
    protected override void ProcessPacket(DispenserCourierPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.mainPlayer.factory;
        var pool = factory?.transport.dispenserPool;
        if (GameMain.mainPlayer.planetId != packet.PlanetId || pool == null)
        {
            return;
        }
        if (packet.DispenserId <= 0 || packet.DispenserId >= pool.Length || pool[packet.DispenserId] == null)
        {
            return;
        }
        var dispenser = pool[packet.DispenserId];
        Multiplayer.Session.Couriers.AddCourier(packet.PlayerId, factory.entityPool[dispenser.entityId].pos, packet.ItemId,
            packet.ItemCount);
        dispenser.pulseSignal = 2;
    }
}
