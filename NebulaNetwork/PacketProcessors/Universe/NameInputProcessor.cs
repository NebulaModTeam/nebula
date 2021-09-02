using NebulaAPI;
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
    class NameInputProcessor : PacketProcessor<NameInputPacket>
    {
        private IPlayerManager playerManager;

        public NameInputProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (Multiplayer.Session.Factories.IsIncomingRequest.On())
                {
                    if (packet.StarId != NebulaModAPI.STAR_NONE)
                    {
                        var star = GameMain.galaxy.StarById(packet.StarId);
                        star.overrideName = packet.Name;
                        star.NotifyOnDisplayNameChange();
                    }
                    else
                    {
                        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                        planet.overrideName = packet.Name;
                        planet.NotifyOnDisplayNameChange();
                    }
                    GameMain.galaxy.NotifyAstroNameChange();
                }
            }
        }
    }
}
