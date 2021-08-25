using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmRemoveOrbitProcessor : PacketProcessor<DysonSwarmRemoveOrbitPacket>
    {
        private PlayerManager playerManager;

        public DysonSwarmRemoveOrbitProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(DysonSwarmRemoveOrbitPacket packet, NebulaConnection conn)
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
                using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.RemoveOrbit(packet.OrbitId);
                }
            }
        }
    }
}
