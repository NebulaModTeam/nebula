#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
{
    private readonly IPlayerManager playerManager;

    public GameHistoryEnqueueTechProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    public override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                using (Multiplayer.Session.History.IsIncomingRequest.On())
                {
                    GameMain.history.EnqueueTech(packet.TechId);
                }
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
        else
        {
            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                GameMain.history.EnqueueTech(packet.TechId);
            }
        }
    }
}
