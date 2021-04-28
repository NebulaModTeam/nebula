using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveFrameProcessor : IPacketProcessor<DysonSphereRemoveFramePacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveFrameProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    if (DysonSphere_Manager.CanRemoveFrame(packet.FrameId, dsl))
                    {
                        dsl.RemoveDysonFrame(packet.FrameId);
                    }
                }
            }
        }
    }
}
