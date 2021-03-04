using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : IPacketProcessor<PlayerAnimationUpdate>
    {
        private PlayerManager playerManager;

        public PlayerAnimationUpdateProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            playerManager.GetPlayerModelById(packet.PlayerId)?.Animator.UpdateState(packet);
        }
    }
}
