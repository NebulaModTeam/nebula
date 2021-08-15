using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddFrameProcessor : PacketProcessor<DysonSphereAddFramePacket>
    {
        private PlayerManager playerManager;

        public DysonSphereAddFrameProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereAddFramePacket packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                using (DysonSphereManager.IsIncomingRequest.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    //Check if target nodes exists (if not, assume that AddNode packet is on the way)
                    if (DysonSphereManager.CanCreateFrame(packet.NodeAId, packet.NodeBId, dsl))
                    {
                        dsl.NewDysonFrame(packet.ProtoId, packet.NodeAId, packet.NodeBId, packet.Euler);
                    }
                    else
                    {
                        DysonSphereManager.QueuedAddFramePackets.Add(packet);
                    }
                }
            }
        }
    }
}

