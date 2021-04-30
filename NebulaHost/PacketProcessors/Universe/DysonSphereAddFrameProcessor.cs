using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddFrameProcessor : IPacketProcessor<DysonSphereAddFramePacket>
    {
        private PlayerManager playerManager;

        public DysonSphereAddFrameProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereAddFramePacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    //Check if target nodes exists (if not, assume that AddNode packet is on the way)
                    if (DysonSphere_Manager.CanCreateFrame(packet.NodeAId, packet.NodeBId, dsl))
                    {
                        dsl.NewDysonFrame(packet.ProtoId, packet.NodeAId, packet.NodeBId, packet.Euler);
                    }
                    else
                    {
                        DysonSphere_Manager.QueuedAddFramePackets.Add(packet);
                    }
                }
            }
        }
    }
}

