using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class AddEntityPreviewRequestProcessor : IPacketProcessor<AddEntityPreviewRequest>
    {
        private PlayerManager playerManager;

        public AddEntityPreviewRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(AddEntityPreviewRequest packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player == null)
            {
                Log.Warn($"Received AddEntityPreviewRequest packet from unknown player connection");
                return;
            }

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                Log.Warn($"planet.factory was null create new one");
                planet.factory = GameMain.data.GetOrCreateFactory(planet);
            }

            using (FactoryManager.EventFromClient.On())
            {
                int nextPrebuildId = FactoryManager.GetNextPrebuildId(packet.PlanetId);
                FactoryManager.SetPrebuildRequest(packet.PlanetId, nextPrebuildId, player.Id);
                PrebuildData prebuild = packet.GetPrebuildData();
                int localPlanetId = GameMain.localPlanet?.id ?? -1;
                if (planet.id == localPlanetId)
                {
                    planet.factory.AddPrebuildDataWithComponents(prebuild);
                }
                else
                {
                    planet.factory.AddPrebuildData(prebuild);

                    // AddPrebuildData is not patched so we need to send the packet manually here
                    LocalPlayer.SendPacket(packet);
                }
            }
        }
    }
}
