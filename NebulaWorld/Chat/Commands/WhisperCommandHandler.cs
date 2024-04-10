#region

using System;
using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class WhisperCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 2)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        var senderUsername = Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "UNKNOWN";
        if (senderUsername == "UNKNOWN" || Multiplayer.Session == null || Multiplayer.Session.LocalPlayer == null)
        {
            window.SendLocalChatMessage("Not connected, can't send message".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

        var recipientUserName = parameters[0];
        var fullMessageBody = string.Join(" ", parameters.Skip(1));
        // first echo what the player typed so they know something actually happened
        ChatManager.Instance.SendChatMessage(ChatManager.FormatChatMessage(DateTime.Now, $"[To {recipientUserName}]", fullMessageBody),
            ChatMessageType.PlayerMessagePrivate);

        var packet = new ChatCommandWhisperPacket(senderUsername, recipientUserName, fullMessageBody);

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            var recipient = Multiplayer.Session.Server.Players.Get(recipientUserName);
            if (recipient == null)
            {
                window.SendLocalChatMessage("Player not found: ".Translate() + recipientUserName,
                    ChatMessageType.CommandErrorMessage);
                return;
            }

            recipient.SendPacket(packet);
        }
        else
        {
            Multiplayer.Session.Network.SendPacket(packet);
        }
    }

    public string GetDescription()
    {
        return string.Format("Send direct message to player. Use /who for valid user names".Translate());
    }

    public string[] GetUsage()
    {
        return new[] { "<player> <message>" };
    }

    public static void SendWhisperToLocalPlayer(string sender, string mesageBody)
    {
        ChatManager.Instance.SendChatMessage(ChatManager.FormatChatMessage(DateTime.Now, $"[From {sender}]", mesageBody),
            ChatMessageType.PlayerMessagePrivate);
    }
}
