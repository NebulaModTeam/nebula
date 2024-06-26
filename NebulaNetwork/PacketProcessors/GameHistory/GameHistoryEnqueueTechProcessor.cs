#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
{
    protected override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                GameMain.history.EnqueueTech(packet.TechId);
            }
            Server.SendPacketExclude(packet, conn);
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
