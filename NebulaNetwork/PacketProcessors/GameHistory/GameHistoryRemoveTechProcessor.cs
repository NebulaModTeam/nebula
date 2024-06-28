#region

using System;
using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryRemoveTechProcessor : PacketProcessor<GameHistoryRemoveTechPacket>
{
    protected override void ProcessPacket(GameHistoryRemoveTechPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }
        using (Multiplayer.Session.History.IsIncomingRequest.On())
        {
            var index = Array.IndexOf(GameMain.history.techQueue, packet.TechId);
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
