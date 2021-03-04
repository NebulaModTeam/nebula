using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class InitialStateProcessor : IPacketProcessor<InitialState>
    {
        public void ProcessPacket(InitialState packet, NebulaConnection conn)
        {
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);
            conn.SendPacket(new SyncComplete());

            // TODO: HIDE PREVIOUSLY OPENED POPUP
        }
    }
}
