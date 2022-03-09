using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using System;
using System.Linq;

namespace NebulaWorld.Chat.Commands
{
    public class WhisperCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ChatCommandUsageException("Not enough arguments!");
            }

            string senderUsername = Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "UNKNOWN";
            if (senderUsername == "UNKNOWN" || Multiplayer.Session == null || Multiplayer.Session.LocalPlayer == null )
            {
                window.SendLocalChatMessage("Not connected, can't send message", ChatMessageType.CommandErrorMessage);
                return;
            }
            
            string recipientUserName = parameters[0];
            string fullMessageBody = string.Join(" ", parameters.Skip(1));
            // first echo what the player typed so they know something actually happened
            ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] [To: {recipientUserName}] : {fullMessageBody}", ChatMessageType.PlayerMessage);

            ChatCommandWhisperPacket packet = new ChatCommandWhisperPacket(senderUsername, recipientUserName, fullMessageBody);
            
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                INebulaPlayer recipient = Multiplayer.Session.Network.PlayerManager.GetConnectedPlayerByUsername(recipientUserName);
                if (recipient == null)
                {
                    window.SendLocalChatMessage($"Player not found: {recipientUserName}", ChatMessageType.CommandErrorMessage);
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
            return $"Send direct message to player. Use {ChatCommandRegistry.CommandPrefix}who for valid user names";
        }

        public string[] GetUsage()
        {
            return new string[] { "<player> <message>" };
        }

        public static void SendWhisperToLocalPlayer(string sender, string mesageBody)
        {
            ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] [{sender} whispered] : {mesageBody}", ChatMessageType.PlayerMessagePrivate);
        }
    }
}