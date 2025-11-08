#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class NewChatMessageProcessor : PacketProcessor<NewChatMessagePacket>
{
    protected override void ProcessPacket(NewChatMessagePacket packet, NebulaConnection conn)
    {
        if (ChatManager.Instance == null)
        {
            Log.Warn("Unable to process chat packet, chat window assets were not loaded properly");
            return;
        }

        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }

        if (packet.MessageType == NebulaModel.DataStructures.Chat.ChatMessageType.SystemWarnMessage)
        {
            Log.Warn(packet.MessageText); // Record system warn message in log file
        }

        if (string.IsNullOrEmpty(packet.UserName))
        {
            // non-player chat
            ChatService.Instance.AddMessage(packet.MessageText, packet.MessageType);
            return;
        }
        var sentAt = packet.SentAt == 0 ? DateTime.Now : DateTime.FromBinary(packet.SentAt);
        ChatService.Instance.AddMessage(packet.MessageText, packet.MessageType, packet.UserName, sentAt);
    }
}
