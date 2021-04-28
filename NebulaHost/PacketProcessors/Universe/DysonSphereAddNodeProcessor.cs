using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddNodeProcessor : IPacketProcessor<DysonSphereAddNodePacket>
    {
        private PlayerManager playerManager;

        public DysonSphereAddNodeProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereAddNodePacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);

                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId)?.NewDysonNode(packet.NodeProtoId, DataStructureExtensions.ToVector3(packet.Position));
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    //Try to add queued Dyson Frames that failed due to the missing nodes
                    DysonSphereAddFramePacket queuedPacked;
                    for (int i = DysonSphere_Manager.QueuedAddFramePackets.Count - 1; i >= 0; i--)
                    {
                        queuedPacked = DysonSphere_Manager.QueuedAddFramePackets[i];
                        if (dsl?.nodePool[queuedPacked.NodeAId]?.id != 0 && dsl?.nodePool[queuedPacked.NodeBId]?.id != 0)
                        {
                            dsl.NewDysonFrame(queuedPacked.ProtoId, queuedPacked.NodeAId, queuedPacked.NodeBId, queuedPacked.Euler);
                            DysonSphere_Manager.QueuedAddFramePackets.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
