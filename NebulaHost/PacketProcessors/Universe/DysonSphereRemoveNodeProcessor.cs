using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveNodeProcessor : IPacketProcessor<DysonSphereRemoveNodePacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveNodeProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereRemoveNodePacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);

                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    if (dsl != null)
                    {
                        int num = 0;
                        DysonNode dysonNode = dsl.nodePool[packet.NodeId];
                        //Remove all frames that are part of the node
                        while (dysonNode.frames.Count > 0)
                        {
                            dsl.RemoveDysonFrame(dysonNode.frames[0].id);
                            if (num++ > 4096)
                            {
                                Assert.CannotBeReached();
                                break;
                            }
                        }
                        //Remove all shells that are part of the node
                        while (dysonNode.shells.Count > 0)
                        {
                            dsl.RemoveDysonShell(dysonNode.shells[0].id);
                            if (num++ > 4096)
                            {
                                Assert.CannotBeReached();
                                break;
                            }
                        }
                        dsl.RemoveDysonNode(packet.NodeId);
                    }
                }
            }
        }
    }
}
