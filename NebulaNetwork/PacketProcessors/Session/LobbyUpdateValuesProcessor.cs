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
            if (IsHost)
            {
                return;
            }

            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            gameDesc.isSandboxMode = packet.IsSandboxMode;

            UIRoot.instance.galaxySelect.gameDesc = gameDesc;
            UIRoot.instance.galaxySelect.SetStarmapGalaxy();
            UIRoot.instance.galaxySelect.sandboxToggle.isOn = packet.IsSandboxMode;
        }
    }
}
