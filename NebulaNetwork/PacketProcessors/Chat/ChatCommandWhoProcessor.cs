#region

using System.Linq;
using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat.Commands;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class ChatCommandWhoProcessor : PacketProcessor<ChatCommandWhoPacket>
{
    protected override void ProcessPacket(ChatCommandWhoPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var recipient = Players.Get(conn);
            var playerDatas = Multiplayer.Session.Server.Players.GetAllPlayerData().ToArray();
            var hostPlayer = Multiplayer.Session.LocalPlayer;
            var resultPayload = WhoCommandHandler.BuildResultPayload(playerDatas, hostPlayer);

            recipient.SendPacket(new ChatCommandWhoPacket(false, resultPayload));
        }
        else
        {
            if (packet.IsRequest)
            {
                Log.Warn("Request packet received for who response");
            }
            ChatManager.Instance.SendChatMessage(packet.ResponsePayload, ChatMessageType.CommandOutputMessage);
        }
    }
}
