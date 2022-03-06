using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Session;
using NebulaWorld.GameStates;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    internal class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        public override void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                conn.SendPacket(new GameStateUpdate(packet.SentTimestamp, GameStatesManager.RealGameTick, GameStatesManager.RealUPS));
            }
            else
            {
                conn.SendPacket(packet);
            }
        }
    }
}
