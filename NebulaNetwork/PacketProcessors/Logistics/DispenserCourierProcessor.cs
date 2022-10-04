using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class DispenserCourierProcessor : PacketProcessor<DispenserCourierPacket>
    {
        public override void ProcessPacket(DispenserCourierPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.mainPlayer.factory;
            DispenserComponent[] pool = factory?.transport.dispenserPool;
            if (pool != null && packet.DispenserId > 0 && packet.DispenserId < pool.Length && pool[packet.DispenserId].id == packet.DispenserId)
            {
                DispenserComponent dispenser = pool[packet.DispenserId];
                Multiplayer.Session.Couriers.AddCourier(packet.PlayerId, factory.entityPool[dispenser.entityId].pos, packet.ItemId, packet.ItemCount);
                dispenser.pulseSignal = 2;
            }
        }
    }
}
