using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeResponseProcessor : IPacketProcessor<HandshakeResponse>
    {
        public void ProcessPacket(HandshakeResponse packet, NebulaConnection conn)
        {
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.PlayerId = packet.LocalPlayerId;
            Log.Warn($"My Player ID: {LocalPlayer.PlayerId}");

            // TODO: Make our own InGameMessageBox class that will keep the reference of the currently open popup internally instead.
            // UIMessageBox.Show("Loading", "Loading state from server, please wait", null, UIMessageBox.INFO);

            foreach (var playerId in packet.OtherPlayerIds)
            {
                SimulatedWorld.SpawnRemotePlayerModel(playerId);
            }

            // TODO: This packet should be sent by the MultiplayerClientSession once the game is loaded not before.
            conn.SendPacket(new SyncComplete());
        }
    }
}
