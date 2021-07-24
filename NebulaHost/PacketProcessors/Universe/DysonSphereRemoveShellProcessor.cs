using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveShellProcessor : PacketProcessor<DysonSphereRemoveShellPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveShellProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);

                using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    if (DysonSphere_Manager.CanRemoveShell(packet.ShellId, dsl))
                    {
                        dsl.RemoveDysonShell(packet.ShellId);
                    }
                }
            }
        }
    }
}
