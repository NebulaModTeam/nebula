using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaHost.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class InitialStateProcessor : IPacketProcessor<InitialState>
    {
        private PlayerManager playerManager;

        public InitialStateProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(InitialState packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            playerManager.PlayerSentInitialState(player, packet);
        }
    }
}
