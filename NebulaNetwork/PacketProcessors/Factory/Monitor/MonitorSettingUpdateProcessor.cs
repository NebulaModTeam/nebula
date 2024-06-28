#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Monitor;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Monitor;

[RegisterPacketProcessor]
internal class MonitorSettingUpdateProcessor : PacketProcessor<MonitorSettingUpdatePacket>
{
    protected override void ProcessPacket(MonitorSettingUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic?.monitorPool;
        if (pool != null && packet.MonitorId > 0 && packet.MonitorId < pool.Length &&
            pool[packet.MonitorId].id == packet.MonitorId)
        {
            using (Multiplayer.Session.Warning.IsIncomingMonitorPacket.On())
            {
                switch (packet.Event)
                {
                    case MonitorSettingEvent.SetPassColorId:
                        pool[packet.MonitorId].SetPassColorId((byte)packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetFailColorId:
                        pool[packet.MonitorId].SetFailColorId((byte)packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetPassOperator:
                        pool[packet.MonitorId].SetPassOperator((byte)packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetMonitorMode:
                        pool[packet.MonitorId].SetMonitorMode((byte)packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetSystemWarningMode:
                        pool[packet.MonitorId].SetSystemWarningMode(packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetSystemWarningSignalId:
                        pool[packet.MonitorId].SetSystemWarningSignalId(packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetCargoFilter:
                        pool[packet.MonitorId].SetCargoFilter(packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetTargetCargoBytes:
                        pool[packet.MonitorId].SetTargetCargoBytes(packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetPeriodTickCount:
                        pool[packet.MonitorId].SetPeriodTickCount(packet.Parameter1);
                        break;

                    case MonitorSettingEvent.SetTargetBelt:
                        pool[packet.MonitorId].SetTargetBelt(packet.Parameter1, packet.Parameter2);
                        break;

                    case MonitorSettingEvent.SetSpawnOperator:
                        pool[packet.MonitorId].SetSpawnOperator((byte)packet.Parameter1);
                        break;

                    default:
                        Log.Warn($"MonitorSettingUpdatePacket: Unknown MonitorSettingEvent {packet.Event}");
                        break;
                }

                //Update UI Window too if it is viewing the current monitor
                var uIMonitor = UIRoot.instance.uiGame.monitorWindow;
                if (uIMonitor.monitorId != packet.MonitorId || uIMonitor.factory == null ||
                    uIMonitor.factory.planetId != packet.PlanetId)
                {
                    return;
                }
                switch (packet.Event)
                {
                    case MonitorSettingEvent.SetMonitorMode:
                        if (packet.Parameter1 == 0)
                        {
                            uIMonitor.TryCloseSpeakerPanel();
                        }
                        else
                        {
                            uIMonitor.TryOpenSpeakerPanel();
                        }
                        break;

                    case MonitorSettingEvent.SetSystemWarningSignalId:
                        var sprite = LDB.signals.IconSprite(packet.Parameter1);
                        uIMonitor.iconTagImage.sprite = sprite ? sprite : uIMonitor.tagNotSelectedSprite;
                        break;

                    case MonitorSettingEvent.SetCargoFilter:
                        var sprite2 = LDB.items.Select(packet.Parameter1)?.iconSprite;
                        uIMonitor.cargoFilterImage.sprite = sprite2 ? sprite2 : uIMonitor.cargoFilterNotSelectedSprite;
                        break;

                    case MonitorSettingEvent.SetPeriodTickCount:
                        uIMonitor.updateTimestamp = 0L;
                        break;

                    case MonitorSettingEvent.SetSpawnOperator:
                        uIMonitor.spawnToggle.isOn = packet.Parameter1 > 0;
                        uIMonitor.spawnSwitch.isOn = packet.Parameter1 == 1;
                        break;
                    case MonitorSettingEvent.SetPassColorId:
                        break;
                    case MonitorSettingEvent.SetFailColorId:
                        break;
                    case MonitorSettingEvent.SetPassOperator:
                        break;
                    case MonitorSettingEvent.SetSystemWarningMode:
                        break;
                    case MonitorSettingEvent.SetTargetCargoBytes:
                        break;
                    case MonitorSettingEvent.SetTargetBelt:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(packet), "Unknown MonitorSettingEvent: " + packet.Event);
                }
                uIMonitor.RefreshMonitorWindow();
            }
        }
        else if (pool != null)
        {
            Log.Warn($"MonitorSettingUpdatePacket: Can't find monitor ({packet.PlanetId}, {packet.MonitorId})");
        }
    }
}
