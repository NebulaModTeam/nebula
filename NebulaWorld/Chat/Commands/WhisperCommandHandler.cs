#region

using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class WhisperCommandHandler : IChatCommandHandler
{
    public void Execute(ChatService chatService, string[] parameters)
    {
        if (parameters.Length < 2)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        var senderUsername = Multiplayer.Session?.LocalPlayer?.Data?.Username;
        if (string.IsNullOrEmpty(senderUsername))
        {
            chatService.AddMessage("Not connected, can't send message".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

        var recipientUserName = parameters[0];
        var fullMessageBody = string.Join(" ", parameters.Skip(1));
        // first echo what the player typed so they know something actually happened
        chatService.AddMessage(fullMessageBody, ChatMessageType.PlayerMessagePrivate, $"[To {recipientUserName}]");

        var packet = new ChatCommandWhisperPacket(senderUsername, recipientUserName, fullMessageBody);

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            var recipient = Multiplayer.Session.Server.Players.Get(recipientUserName);
            if (recipient == null)
            {
                chatService.AddMessage("Player not found: ".Translate() + recipientUserName,
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
        return ["<player> <message>"];
    }
}
