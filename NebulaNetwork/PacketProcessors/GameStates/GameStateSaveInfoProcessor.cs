#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.GameStates;

[RegisterPacketProcessor]
public class GameStateSaveInfoProcessor : PacketProcessor<GameStateSaveInfoPacket>
{
    protected override void ProcessPacket(GameStateSaveInfoPacket packet, NebulaConnection conn)
    {
        GameStatesManager.LastSaveTime = packet.LastSaveTime;
        Log.Info("LastSaveTime: " + DateTimeOffset.FromUnixTimeSeconds(packet.LastSaveTime).LocalDateTime.ToString());
    }
}
