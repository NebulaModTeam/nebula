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
    internal class DispenserSettingProcessor : PacketProcessor<DispenserSettingPacket>
    {
        public override void ProcessPacket(DispenserSettingPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            DispenserComponent[] pool = factory?.transport.dispenserPool;
            if (pool != null && packet.DispenserId > 0 && packet.DispenserId < pool.Length && pool[packet.DispenserId].id == packet.DispenserId)
            {
                ref DispenserComponent dispenserComponent = ref pool[packet.DispenserId];

                using (Multiplayer.Session.StationsUI.IsIncomingRequest.On())
                {
                    switch (packet.Event)
                    {
                        case EDispenserSettingEvent.SetCourierCount:
                            int newCourierCount = packet.Parameter1;
                            if (dispenserComponent.workCourierCount > newCourierCount)
                            {
                                string warnText = string.Format("{0} [{1}] Working courier decrease from {2} to {3}",
                                    GameMain.galaxy.PlanetById(packet.PlanetId).displayName, packet.DispenserId, dispenserComponent.workCourierCount, newCourierCount);
                                Log.Debug(warnText);
                                dispenserComponent.workCourierCount = newCourierCount;
                            }
                            dispenserComponent.idleCourierCount = newCourierCount - dispenserComponent.workCourierCount;
                            break;

                        case EDispenserSettingEvent.ToggleAutoReplenish:
                            dispenserComponent.courierAutoReplenish = packet.Parameter1 != 0;
                            break;

                        case EDispenserSettingEvent.SetMaxChargePower:
                            float value = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                            factory.powerSystem.consumerPool[dispenserComponent.pcId].workEnergyPerTick = (long)(5000.0 * value + 0.5);
                            break;

                        case EDispenserSettingEvent.SetFilter:
                            int filter = packet.Parameter1;
                            if (dispenserComponent.filter != filter)
                            {
                                dispenserComponent.filter = filter;
                                factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                            }
                            break;

                        case EDispenserSettingEvent.SetPlayerDeliveryMode:
                            EPlayerDeliveryMode playerDeliveryMode = (EPlayerDeliveryMode)packet.Parameter1;
                            if (dispenserComponent.playerMode != playerDeliveryMode)
                            {
                                dispenserComponent.playerMode = playerDeliveryMode;
                                factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                            }
                            break;

                        case EDispenserSettingEvent.SetStorageDeliveryMode:
                            EStorageDeliveryMode storageDeliveryMode = (EStorageDeliveryMode)packet.Parameter1;
                            if (dispenserComponent.storageMode != storageDeliveryMode)
                            {
                                dispenserComponent.storageMode = storageDeliveryMode;
                                factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                            }
                            break;

                        default:
                            Log.Warn($"DispenserSettingPacket: Unkown DispenserSettingEvent {packet.Event}");
                            break;
                    }

                    UIDispenserWindow uiWindow = UIRoot.instance.uiGame.dispenserWindow;
                    if (uiWindow.dispenserId == packet.DispenserId && uiWindow.factory?.planetId == packet.PlanetId)
                    {
                        uiWindow.OnDispenserIdChange();
                    }
                }
            }
            else if (pool != null)
            {
                Log.Warn($"DispenserSettingPacket: Can't find dispenser ({packet.PlanetId}, {packet.DispenserId})");
            }
        }
    }
}
