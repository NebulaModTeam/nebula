using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddShellProcessor : PacketProcessor<DysonSphereAddShellPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereAddShellProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereAddShellPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId)?.NewDysonShell(packet.ProtoId, new List<int>(packet.NodeIds));
                }
            }
        }
    }
}

