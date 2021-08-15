using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryRemoveTechProcessor : PacketProcessor<GameHistoryRemoveTechPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryRemoveTechProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryRemoveTechPacket packet, NebulaConnection conn)
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
                using (GameDataHistoryManager.IsIncomingRequest.On())
                {
                    int index = System.Array.IndexOf(GameMain.history.techQueue, packet.TechId);
                    //sanity: packet wanted to remove tech, which is not queued on this client, ignore it
                    if (index < 0)
                    {
                        Log.Warn($"ProcessPacket: TechId: {packet.TechId} was not in queue, discarding packet");
                        return;
                    }
                    GameMain.history.RemoveTechInQueue(index);
                }
            }
        }
    }
}
