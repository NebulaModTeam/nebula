using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddFrameProcessor : PacketProcessor<DysonSphereAddFramePacket>
    {
        private IPlayerManager playerManager;

        public DysonSphereAddFrameProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereAddFramePacket packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
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
                using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    //Check if target nodes exists (if not, assume that AddNode packet is on the way)
                    if (Multiplayer.Session.DysonSpheres.CanCreateFrame(packet.NodeAId, packet.NodeBId, dsl))
                    {
                        dsl.NewDysonFrame(packet.ProtoId, packet.NodeAId, packet.NodeBId, packet.Euler);
                    }
                    else
                    {
                        Multiplayer.Session.DysonSpheres.QueuedAddFramePackets.Add(packet);
                    }
                }
            }
        }
    }
}

