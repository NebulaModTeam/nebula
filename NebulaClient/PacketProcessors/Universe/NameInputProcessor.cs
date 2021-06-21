using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Universe
{
    /*
     * Receives change event for name of planet or star and applies the change
    */
    [RegisterPacketProcessor]
    class NameInputProcessor : IPacketProcessor<NameInputPacket>
    {
        public void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
        {
            // We don't want to process events that we sent
            if (packet.AuthorId == LocalPlayer.PlayerId)
            {
                return;
            }
            using (FactoryManager.EventFromServer.On())
            {
                if (packet.StellarId >= GameMain.galaxy.stars.Length)
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
