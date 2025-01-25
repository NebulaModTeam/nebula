#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
internal class GlobalGameDataResponseProcessor : PacketProcessor<GlobalGameDataResponse>
{
    protected override void ProcessPacket(GlobalGameDataResponse packet, NebulaConnection conn)
    {
        if (IsHost) return;

        // Store the binary data in GameStatesManager then later overwrite those system in GameData.NewGame
        Multiplayer.Session.State.ImportGlobalGameData(packet);
    }
}
