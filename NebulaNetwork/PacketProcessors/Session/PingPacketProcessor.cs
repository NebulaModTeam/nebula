using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Session;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    internal class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        public override void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                conn.SendPacket(new GameStateUpdate(packet.SentTimestamp, GameMain.gameTick, (float)FPSController.currentUPS));
            }
            else
            {
                conn.SendPacket(packet);
            }
        }
    }
}
