#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryResearchUpdateProcessor : PacketProcessor<GameHistoryResearchUpdatePacket>
{
    protected override void ProcessPacket(GameHistoryResearchUpdatePacket packet, NebulaConnection conn)
    {
        var data = GameMain.data.history;
        if (packet.TechId != data.currentTech)
        {
            Log.Warn($"CurrentTech mismatch! Server:{packet.TechId} Local:{data.currentTech}");
            //Replace currentTech to match with server
            data.currentTech = packet.TechId;
            data.techQueue[0] = packet.TechId;
        }
        var state = data.techStates[data.currentTech];
        state.hashUploaded = packet.HashUploaded;
        state.hashNeeded = packet.HashNeeded;
        data.techStates[data.currentTech] = state;
        Multiplayer.Session.Statistics.TechHashedFor10Frames = packet.TechHashedFor10Frames;

        if (packet.TechQueueLength != GameMain.history.techQueueLength)
        {
            // TechQueue length mismatch. Ask from server to get a full queue to stay in sync
            conn.SendPacket(new GameHistoryTechQueueSyncRequest([]));
        }
    }
}
