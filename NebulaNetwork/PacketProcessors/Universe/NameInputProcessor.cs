using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using FactoryManager = NebulaWorld.Factory.FactoryManager;

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
                using (FactoryManager.Instance.IsIncomingRequest.On())
                {
                    if (packet.StarId != FactoryManager.Instance.STAR_NONE)
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
