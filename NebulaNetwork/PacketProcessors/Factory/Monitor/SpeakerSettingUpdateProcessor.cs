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
internal class SpeakerSettingUpdateProcessor : PacketProcessor<SpeakerSettingUpdatePacket>
{
    protected override void ProcessPacket(SpeakerSettingUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.digitalSystem?.speakerPool;
        if (pool != null && packet.SpeakerId > 0 && packet.SpeakerId < pool.Length &&
            pool[packet.SpeakerId].id == packet.SpeakerId)
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
                        var p0 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                        pool[packet.SpeakerId].SetLength(p0);
                        break;

                    case SpeakerSettingEvent.SetRepeat:
                        pool[packet.SpeakerId].SetRepeat(packet.Parameter1 != 0);
                        break;

                    case SpeakerSettingEvent.SetFalloffRadius:
                        var p1 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter1), 0);
                        var p2 = BitConverter.ToSingle(BitConverter.GetBytes(packet.Parameter2), 0);
                        pool[packet.SpeakerId].SetFalloffRadius(p1, p2);
                        break;

                    default:
                        Log.Warn($"SpeakerSettingUpdatePacket: Unknown SpeakerSettingEvent {packet.Event}");
                        break;
                }

                //Update UI Panel too if it is viewing the current speaker
                var uISpeaker = UIRoot.instance.uiGame.monitorWindow.speakerPanel;
                if (uISpeaker.speakerId != packet.SpeakerId || uISpeaker.factory == null ||
                    uISpeaker.factory.planetId != packet.PlanetId)
                {
                    return;
                }
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
                            var audioId = uISpeaker.factory.entityPool[pool[packet.SpeakerId].entityId].audioId;
                            uISpeaker.factory.planet.audio?.ChangeAudioDataFalloff(audioId,
                                pool[packet.SpeakerId].falloffRadius0, pool[packet.SpeakerId].falloffRadius1);
                        }
                        break;
                    case SpeakerSettingEvent.SetTone:
                        break;
                    case SpeakerSettingEvent.SetVolume:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(packet), "Unknown SpeakerSettingEvent: " + packet.Event);
                }
                uISpeaker.RefreshSpeakerPanel();
            }
        }
        else if (pool != null)
        {
            Log.Warn($"SpeakerSettingUpdatePacket: Can't find speaker ({packet.PlanetId}, {packet.SpeakerId})");
        }
    }
}
