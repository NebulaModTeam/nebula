#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class DispenserSettingProcessor : PacketProcessor<DispenserSettingPacket>
{
    protected override void ProcessPacket(DispenserSettingPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        var pool = factory?.transport.dispenserPool;
        if (pool != null && packet.DispenserId > 0 && packet.DispenserId < pool.Length &&
            pool[packet.DispenserId].id == packet.DispenserId)
        {
            ref var dispenserComponent = ref pool[packet.DispenserId];

            using (Multiplayer.Session.StationsUI.IsIncomingRequest.On())
            {
                switch (packet.Event)
                {
                    case EDispenserSettingEvent.SetCourierCount:
                        var newCourierCount = packet.Parameter1;
                        if (dispenserComponent.workCourierCount > newCourierCount)
                        {
                            var warnText =
                                $"{GameMain.galaxy.PlanetById(packet.PlanetId).displayName} [{packet.DispenserId}] Working courier decrease from {dispenserComponent.workCourierCount} to {newCourierCount}";
                            Log.Debug(warnText);
                            dispenserComponent.workCourierCount = newCourierCount;
                        }
                        dispenserComponent.idleCourierCount = newCourierCount - dispenserComponent.workCourierCount;
                        break;

                    case EDispenserSettingEvent.ToggleAutoReplenish:
                        dispenserComponent.courierAutoReplenish = packet.Parameter1 != 0;
                        break;

                    case EDispenserSettingEvent.SetMaxChargePower:
                        var value = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                        factory.powerSystem.consumerPool[dispenserComponent.pcId].workEnergyPerTick =
                            (long)(5000.0 * value + 0.5);
                        break;

                    case EDispenserSettingEvent.SetFilter:
                        var filter = packet.Parameter1;
                        if (dispenserComponent.filter != filter)
                        {
                            dispenserComponent.filter = filter;
                            factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                        }
                        break;

                    case EDispenserSettingEvent.SetPlayerDeliveryMode:
                        var playerDeliveryMode = (EPlayerDeliveryMode)packet.Parameter1;
                        if (dispenserComponent.playerMode != playerDeliveryMode)
                        {
                            dispenserComponent.playerMode = playerDeliveryMode;
                            factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                        }
                        break;

                    case EDispenserSettingEvent.SetStorageDeliveryMode:
                        var storageDeliveryMode = (EStorageDeliveryMode)packet.Parameter1;
                        if (dispenserComponent.storageMode != storageDeliveryMode)
                        {
                            dispenserComponent.storageMode = storageDeliveryMode;
                            factory.transport.RefreshDispenserTraffic(packet.DispenserId);
                        }
                        break;
                    case EDispenserSettingEvent.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(packet), "Unknown DispenserSettingPacket event: " +
                                                                              packet.Event);
                }

                var uiWindow = UIRoot.instance.uiGame.dispenserWindow;
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
