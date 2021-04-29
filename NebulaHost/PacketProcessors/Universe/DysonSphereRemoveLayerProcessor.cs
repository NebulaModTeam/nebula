using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveLayerProcessor : IPacketProcessor<DysonSphereRemoveLayerPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveLayerProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereRemoveLayerPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.RemoveLayer(packet.LayerId);
                }
            }
        }
    }
}
