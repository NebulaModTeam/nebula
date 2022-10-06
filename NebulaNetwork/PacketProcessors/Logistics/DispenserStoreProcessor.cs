using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using System;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class DispenserStoreProcessor : PacketProcessor<DispenserStorePacket>
    {
        public override void ProcessPacket(DispenserStorePacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            DispenserComponent[] pool = factory?.transport.dispenserPool;
            if (pool != null && packet.DispenserId > 0 && packet.DispenserId < pool.Length && pool[packet.DispenserId].id == packet.DispenserId)
            {
                ref DispenserComponent dispenser = ref pool[packet.DispenserId];
                dispenser.holdupItemCount = packet.HoldupItemCount;
                for (int i = 0; i < packet.HoldupItemCount; i++)
                {
                    dispenser.holdupPackage[i].itemId = packet.ItemIds[i];
                    dispenser.holdupPackage[i].count = packet.Counts[i];
                    dispenser.holdupPackage[i].inc = packet.Incs[i];
                }
                Array.Clear(dispenser.holdupPackage, dispenser.holdupItemCount, dispenser.holdupPackage.Length - dispenser.holdupItemCount);
            }
            else if (pool != null)
            {
                Log.Warn($"DispenserSettingPacket: Can't find dispenser ({packet.PlanetId}, {packet.DispenserId})");
            }
        }
    }
}
