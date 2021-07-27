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
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (DysonSphereManager.IsIncomingRequest.On())
                {
                    DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                    if (DysonSphereManager.CanRemoveShell(packet.ShellId, dsl))
                    {
                        dsl.RemoveDysonShell(packet.ShellId);
                    }
                }
            }
        }
    }
}
