using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using NebulaModel.Logger;
using NebulaModel.Packets.Processors;
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
            Log.Info($"Processing DysonSphere remove shell notification for system {GameMain.data.galaxy.stars[packet.StarIndex].name} (Index: {GameMain.data.galaxy.stars[packet.StarIndex].index})");
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                DysonSphere_Manager.IncomingDysonSpherePacket = true;
                DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                if (DysonSphere_Manager.CanRemoveShell(packet.ShellId, dsl))
                {
                    dsl.RemoveDysonShell(packet.ShellId);
                }
                DysonSphere_Manager.IncomingDysonSpherePacket = false;
            }
        }
    }
}
