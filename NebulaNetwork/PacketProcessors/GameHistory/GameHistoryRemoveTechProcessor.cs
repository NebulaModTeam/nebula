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
    public GameHistoryRemoveTechProcessor()
    {
    }

    protected override void ProcessPacket(GameHistoryRemoveTechPacket packet, NebulaConnection conn)
    {
        var valid = true;
        if (IsHost)
        {
            var player = Players.Get(conn);
            if (player != null)
            {
                Server.SendPacketExclude(packet, conn);
            }
            else
            {
                valid = false;
            }
        }

        if (!valid)
        {
            return;
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
