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
    public class DysonSphereAddLayerProcessor : PacketProcessor<DysonSphereAddLayerPacket>
    {
        private IPlayerManager playerManager;

        public DysonSphereAddLayerProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereAddLayerPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.AddLayer(packet.OrbitRadius, packet.OrbitRotation.ToQuaternion(), packet.OrbitAngularSpeed);
                }
            }
        }
    }
}
