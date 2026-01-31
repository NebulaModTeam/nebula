#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class MarkerSettingUpdateProcessor : PacketProcessor<MarkerSettingUpdatePacket>
{
    protected override void ProcessPacket(MarkerSettingUpdatePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }

        var markers = factory.digitalSystem?.markers;
        if (markers == null || packet.MarkerId <= 0 || packet.MarkerId >= markers.cursor)
        {
            return;
        }

        ref var marker = ref markers.buffer[packet.MarkerId];
        if (marker.id != packet.MarkerId)
        {
            return;
        }

        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.Warning.IsIncomingMarkerPacket.On())
        {
            switch (packet.Event)
            {
                case MarkerSettingEvent.SetName:
                    marker.name = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetWord:
                    marker.word = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetTags:
                    marker.tags = packet.StringValue;
                    break;

                case MarkerSettingEvent.SetTodoContent:
                    // Create todo if it doesnt exist (marker was created without memo)
                    if (marker.todo == null)
                    {
                        marker.todo = GameMain.data?.galacticDigital?.AddTodoModule(ETodoModuleOwnerType.Entity, marker.gid);
                    }
                    if (marker.todo != null)
                    {
                        marker.todo.content = packet.StringValue;
                        if (packet.ColorData != null)
                        {
                            marker.todo.contentColorIndex = packet.ColorData;
                        }
                    }
                    break;

                case MarkerSettingEvent.SetIcon:
                    marker.icon = packet.IntValue;
                    break;

                case MarkerSettingEvent.SetColor:
                    marker.color = (byte)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetVisibility:
                    marker.visibility = (EMarkerVisibility)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetDetailLevel:
                    marker.detailLevel = (EMarkerDetailLevel)packet.IntValue;
                    break;

                case MarkerSettingEvent.SetHeight:
                    marker.SetHeight(factory.entityPool, packet.FloatValue);
                    break;

                case MarkerSettingEvent.SetRadius:
                    marker.SetRadius(packet.FloatValue);
                    break;
            }

            try
            {
                marker.InternalUpdate(factory.digitalSystem?.digitalSignalPool);
            }
            catch
            {
            }
        }
    }
}
