#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.GameStates;

[RegisterPacketProcessor]
internal class GameStateRequestProcessor : PacketProcessor<GameStateRequest>
{
    protected override void ProcessPacket(GameStateRequest packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            conn.SendPacket(
                new GameStateUpdate(packet.SentTimestamp, GameStatesManager.RealGameTick, GameStatesManager.RealUPS));
        }
        else
        {
            conn.SendPacket(packet);
        }
    }
}
