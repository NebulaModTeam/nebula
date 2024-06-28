#region

using System;
using NebulaAPI;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class BeltReverseProcessor : BasePacketProcessor<BeltReverseRequestPacket>
{
    public override void ProcessPacket(BeltReverseRequestPacket packet, INebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId).factory;
        if (factory == null)
        {
            return;
        }

        if (IsHost)
        {
            var starId = packet.PlanetId / 100;
            Multiplayer.Session.Server.SendPacketToStar(packet, starId);
        }

        using (NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.On())
        {
            NebulaModAPI.MultiplayerSession.Factories.EventFactory = factory;
            NebulaModAPI.MultiplayerSession.Factories.TargetPlanet = packet.PlanetId;
            NebulaModAPI.MultiplayerSession.Factories.PacketAuthor = packet.AuthorId;
            if (IsHost)
            {
                // Load planet model
                NebulaModAPI.MultiplayerSession.Factories.AddPlanetTimer(packet.PlanetId);
            }

            var beltWindow = UIRoot.instance.uiGame.beltWindow;
            try
            {
                beltWindow._Close(); // close the window first to avoid changing unrelated variables when setting beltId
                beltWindow.factory = factory;
                beltWindow.traffic = factory.cargoTraffic;
                beltWindow.player = GameMain.mainPlayer;
                beltWindow.beltId = packet.BeltId;
                beltWindow.OnReverseButtonClick(0);
                beltWindow._Close();
            }
            catch (Exception e)
            {
                Log.Warn(e);
                beltWindow._tmp_ids.Clear();
                beltWindow._tmp_cargos.Clear();
                beltWindow._Close();
            }

            NebulaModAPI.MultiplayerSession.Factories.EventFactory = null;
            NebulaModAPI.MultiplayerSession.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            NebulaModAPI.MultiplayerSession.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}
