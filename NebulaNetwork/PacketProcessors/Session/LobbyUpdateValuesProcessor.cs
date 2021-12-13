using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class LobbyUpdateValuesProcessor:PacketProcessor<LobbyUpdateValues>
    {
        public override void ProcessPacket(LobbyUpdateValues packet, NebulaConnection conn)
        {
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);

            UIRoot.instance.galaxySelect.gameDesc = gameDesc;
            UIRoot.instance.galaxySelect.SetStarmapGalaxy();
        }
    }
}
