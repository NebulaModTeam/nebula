using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveShellProcessor : IPacketProcessor<DysonSphereRemoveShellPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveShellProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
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
