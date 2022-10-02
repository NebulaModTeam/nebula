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
    internal class DispenserAddTakeProcessor : PacketProcessor<DispenserAddTakePacket>
    {
        public override void ProcessPacket(DispenserAddTakePacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            EntityData[] pool = factory?.entityPool;
            if (pool != null && packet.EntityId > 0 && packet.EntityId < pool.Length && pool[packet.EntityId].id == packet.EntityId)
            {
                using (Multiplayer.Session.Storage.IsIncomingRequest.On())
                {
                    switch (packet.AddTakeEvent)
                    {
                        case EDispenserAddTakeEvent.ManualAdd:
                            factory.InsertIntoStorage(packet.EntityId, packet.ItemId, packet.ItemCount, packet.ItemInc, out int _, false);
                            break;

                        case EDispenserAddTakeEvent.ManualTake:
                            factory.PickFromStorage(packet.EntityId, packet.ItemId, packet.ItemCount, out int _);
                            break;

                        case EDispenserAddTakeEvent.CourierAdd:
                            int addCount = factory.InsertIntoStorage(packet.EntityId, packet.ItemId, packet.ItemCount, packet.ItemInc, out int _, false);
                            int remainCount = packet.ItemCount - addCount;
                            if (remainCount > 0)
                            {
                                Log.Warn($"{GameMain.galaxy.PlanetById(packet.PlanetId)} - CourierAdd remain: {remainCount}");
                            }
                            break;

                        case EDispenserAddTakeEvent.CourierTake:
                            factory.PickFromStorage(packet.EntityId, packet.ItemId, packet.ItemCount, out int _);
                            break;
                    }
                }
            }
            else if (pool != null)
            {
                Log.Warn($"DispenserSettingPacket: Can't find dispenser ({packet.PlanetId}, {packet.EntityId})");
            }
        }
    }
}
