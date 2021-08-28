using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveFrameProcessor : PacketProcessor<DysonSphereRemoveFramePacket>
    {
        private IPlayerManager playerManager;

        public DysonSphereRemoveFrameProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
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
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    if (Multiplayer.Session.DysonSpheres.CanRemoveFrame(packet.FrameId, dsl))
                    {
                        dsl.RemoveDysonFrame(packet.FrameId);
                    }
                }
            }
        }
    }
}
