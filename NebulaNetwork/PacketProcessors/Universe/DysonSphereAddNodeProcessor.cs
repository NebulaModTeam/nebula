using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddNodeProcessor : PacketProcessor<DysonSphereAddNodePacket>
    {
        private IPlayerManager playerManager;

        public DysonSphereAddNodeProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereAddNodePacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId)?.NewDysonNode(packet.NodeProtoId, DataStructureExtensions.ToVector3(packet.Position));
                    // Try to add frames that failed due to the missing nodes
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);

                    // DysonSphereLayer is missing, we can't do anything now.
                    if (dsl == null)
                    {
                        NebulaModel.Logger.Log.Warn("Could not add Dyson Sphere Node, DysonSphereLayer is null.");
                        return;
                    }

                    // Try to add queued Dyson Frames that failed due to the missing nodes
                    DysonSphereAddFramePacket queuedPacked;
                    for (int i = Multiplayer.Session.DysonSpheres.QueuedAddFramePackets.Count - 1; i >= 0; i--)
                    {
                        queuedPacked = Multiplayer.Session.DysonSpheres.QueuedAddFramePackets[i];
                        if (dsl.nodePool[queuedPacked.NodeAId].id != 0 && dsl.nodePool[queuedPacked.NodeBId].id != 0)
                        {
                            dsl.NewDysonFrame(queuedPacked.ProtoId, queuedPacked.NodeAId, queuedPacked.NodeBId, queuedPacked.Euler);
                            Multiplayer.Session.DysonSpheres.QueuedAddFramePackets.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
