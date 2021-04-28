using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmAddOrbitProcessor : IPacketProcessor<DysonSwarmAddOrbitPacket>
    {
        private PlayerManager playerManager;

        public DysonSwarmAddOrbitProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(DysonSwarmAddOrbitPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
                using (DysonSphere_Manager.IncomingDysonSwarmPacket.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.NewOrbit(packet.Radius, DataStructureExtensions.ToQuaternion(packet.Rotation));
                }
            }
        }
    }
}
