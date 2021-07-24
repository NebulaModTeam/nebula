using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Universe
{
    /*
     * Receives change event for name of planet or star and applies the change
    */
    [RegisterPacketProcessor]
    class NameInputProcessor : PacketProcessor<NameInputPacket>
    {
        private PlayerManager playerManager;

        public NameInputProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (FactoryManager.IsIncomingRequest.On())
                {
                    // If stellarId > 100 then it must be a planet
                    if (packet.StellarId > 100)
                    {
                        var planet = GameMain.galaxy.PlanetById(packet.StellarId);
                        planet.overrideName = packet.Name;
                        planet.NotifyOnDisplayNameChange();
                    }
                    else
                    {
                        var star = GameMain.galaxy.StarById(packet.StellarId);
                        star.overrideName = packet.Name;
                        star.NotifyOnDisplayNameChange();
                    }
                    GameMain.galaxy.NotifyAstroNameChange();
                }
            }
        }
    }
}
