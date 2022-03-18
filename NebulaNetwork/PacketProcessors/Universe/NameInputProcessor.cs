using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    /*
     * Receives change event for name of planet or star and applies the change
    */
    [RegisterPacketProcessor]
    internal class NameInputProcessor : PacketProcessor<NameInputPacket>
    {
        private readonly IPlayerManager playerManager;

        public NameInputProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                // If in lobby, apply change to UI galaxy
                GalaxyData galaxyData = Multiplayer.Session.IsInLobby ? UIRoot.instance.galaxySelect.starmap.galaxyData : GameMain.galaxy;
                if (galaxyData == null)
                {
                    return;
                }

                for (int i = 0; i < packet.Names.Length; i++)
                {
                    if (packet.StarIds[i] != NebulaModAPI.STAR_NONE)
                    {
                        StarData star = galaxyData.StarById(packet.StarIds[i]);
                        star.overrideName = packet.Names[i];
                        star.NotifyOnDisplayNameChange();
                        Log.Debug($"star{star.id}: {star.name} -> {star.overrideName}");
                    }
                    else
                    {
                        PlanetData planet = galaxyData.PlanetById(packet.PlanetIds[i]);
                        planet.overrideName = packet.Names[i];
                        planet.NotifyOnDisplayNameChange();
                        Log.Debug($"planet{planet.id}: {planet.name} -> {planet.overrideName}");
                    }
                }
                galaxyData.NotifyAstroNameChange();
                if (Multiplayer.Session.IsInLobby)
                {
                    // Refresh star name in lobby
                    UIRoot.instance.galaxySelect.starmap.OnGalaxyDataReset();
                }
            }
        }
    }
}
