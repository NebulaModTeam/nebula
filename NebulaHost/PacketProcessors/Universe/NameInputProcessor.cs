using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Universe
{
    /*
     * Receives change event for name of planet or star and applies the change
    */
    [RegisterPacketProcessor]
    class NameInputProcessor : IPacketProcessor<NameInputPacket>
    {
        public void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
        {
            using (FactoryManager.EventFromClient.On())
            {
                if (packet.StarId != FactoryManager.STAR_NONE)
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

                // Relay packet to other players
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
