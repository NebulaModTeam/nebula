﻿using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Monitor;
using NebulaWorld;
using System;

namespace NebulaNetwork.PacketProcessors.Factory.Monitor
{
    [RegisterPacketProcessor]
    internal class SpeakerSettingUpdateProcessor : PacketProcessor<SpeakerSettingUpdatePacket>
    {
        public override void ProcessPacket(SpeakerSettingUpdatePacket packet, NebulaConnection conn)
        {
            SpeakerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.digitalSystem?.speakerPool;
            if (pool != null && packet.SpeakerId > 0 && packet.SpeakerId < pool.Length && pool[packet.SpeakerId].id == packet.SpeakerId)
            {
                using (Multiplayer.Session.Warning.IsIncomingMonitorPacket.On())
                {

                    switch (packet.Event)
                    {
                        case SpeakerSettingEvent.SetTone:
                            pool[packet.SpeakerId].SetTone(packet.Parameter1);
                            break;

                        case SpeakerSettingEvent.SetVolume:
                            pool[packet.SpeakerId].SetVolume(packet.Parameter1);
                            break;

                        case SpeakerSettingEvent.SetPitch:
                            pool[packet.SpeakerId].SetPitch(packet.Parameter1);
                            break;

                        case SpeakerSettingEvent.SetLength:
                            float p0 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                            pool[packet.SpeakerId].SetLength(p0);
                            break;

                        case SpeakerSettingEvent.SetRepeat:
                            pool[packet.SpeakerId].SetRepeat(packet.Parameter1 == 0 ? false : true);
                            break;

                        case SpeakerSettingEvent.SetFalloffRadius:
                            float p1 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                            float p2 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter2), 0);
                            pool[packet.SpeakerId].SetFalloffRadius(p1, p2);
                            break;

                        default:
                            Log.Warn($"SpeakerSettingUpdatePacket: Unkown SpeakerSettingEvent {packet.Event}");
                            break;
                    }

                    //Update UI Panel too if it is viewing the current speaker
                    UISpeakerPanel uISpeaker = UIRoot.instance.uiGame.monitorWindow.speakerPanel;
                    if (uISpeaker.speakerId == packet.SpeakerId && uISpeaker.factory != null && uISpeaker.factory.planetId == packet.PlanetId)
                    {
                        switch (packet.Event)
                        {
                            case SpeakerSettingEvent.SetRepeat:
                            case SpeakerSettingEvent.SetPitch:
                            case SpeakerSettingEvent.SetLength:
                                uISpeaker.valueChangeCountDown = 0.5f;
                                break;

                            case SpeakerSettingEvent.SetFalloffRadius:
                                if (uISpeaker.factory.entityPool != null)
                                {
                                    int audioId = uISpeaker.factory.entityPool[pool[packet.SpeakerId].entityId].audioId;
                                    uISpeaker.factory.planet.audio?.ChangeAudioDataFalloff(audioId, pool[packet.SpeakerId].falloffRadius0, pool[packet.SpeakerId].falloffRadius1);
                                }
                                break;

                            default:
                                break;
                        }
                        uISpeaker.RefreshSpeakerPanel();
                    }
                }
            }
            else if (pool != null)
            {
                Log.Warn($"SpeakerSettingUpdatePacket: Can't find speaker ({packet.PlanetId}, {packet.SpeakerId})");
            }
        }
    }
}
