#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSUpdateRouteProcessor : PacketProcessor<ILSUpdateRoute>
{
    protected override void ProcessPacket(ILSUpdateRoute packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Ships.PatchLockILS.On())
        {
            var galacticTransport = GameMain.data.galacticTransport;
            switch (packet.Type)
            {
                case ILSUpdateRoute.ERouteEvent.AddStation2StationRoute:
                    galacticTransport.AddStation2StationRoute(packet.Id0, packet.Id1);
                    break;

                case ILSUpdateRoute.ERouteEvent.RemoveStation2StationRoute_Single:
                    galacticTransport.RemoveStation2StationRoute(packet.Id0);
                    break;

                case ILSUpdateRoute.ERouteEvent.RemoveStation2StationRoute_Pair:
                    galacticTransport.RemoveStation2StationRoute(packet.Id0, packet.Id1);
                    break;

                case ILSUpdateRoute.ERouteEvent.AddAstro2AstroRoute:
                    galacticTransport.AddAstro2AstroRoute(packet.Id0, packet.Id1, packet.ItemId);
                    break;

                case ILSUpdateRoute.ERouteEvent.RemoveAstro2AstroRoute:
                    galacticTransport.RemoveAstro2AstroRoute(packet.Id0, packet.Id1, packet.ItemId);
                    break;

                case ILSUpdateRoute.ERouteEvent.SetAstro2AstroRouteEnable:
                    galacticTransport.SetAstro2AstroRouteEnable(packet.Id0, packet.Id1, packet.ItemId, packet.Enable);
                    break;

                case ILSUpdateRoute.ERouteEvent.SetAstro2AstroRouteComment:
                    galacticTransport.SetAstro2AstroRouteComment(packet.Id0, packet.Id1, packet.ItemId, packet.Comment);
                    break;
            }
        }
    }
}
