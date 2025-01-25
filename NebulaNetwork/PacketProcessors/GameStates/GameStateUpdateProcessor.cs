#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameStates;

[RegisterPacketProcessor]
// ReSharper disable once UnusedType.Global
public class GameStateUpdateProcessor : PacketProcessor<GameStateUpdate>
{
    protected override void ProcessPacket(GameStateUpdate packet, NebulaConnection conn)
    {
        Multiplayer.Session.State.ProcessGameStateUpdatePacket(packet.SentTime, packet.GameTick, packet.UnitsPerSecond);
    }
}
