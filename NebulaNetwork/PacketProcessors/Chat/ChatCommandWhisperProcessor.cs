#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class ChatCommandWhisperProcessor : PacketProcessor<ChatCommandWhisperPacket>
{
    protected override void ProcessPacket(ChatCommandWhisperPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            ChatService.Instance.AddMessage(packet.Message, ChatMessageType.PlayerMessagePrivate, $"[From {packet.SenderUsername}]");
        }
        else
        {
            // two cases, simplest is that whisper is meant for host
            if (Multiplayer.Session.LocalPlayer.Data.Username == packet.RecipientUsername)
            {
                ChatService.Instance.AddMessage(packet.Message, ChatMessageType.PlayerMessagePrivate, $"[From {packet.SenderUsername}]");
                return;
            }

            // second case, relay message to recipient
            var recipient = Players.Get(packet.RecipientUsername);
            if (recipient == null)
            {
                Log.Warn($"Recipient not found {packet.RecipientUsername}");
                var sender = Players.Get(conn);
                sender.SendPacket(new ChatCommandWhisperPacket("SYSTEM".Translate(), packet.SenderUsername,
                    string.Format("User not found {0}".Translate(), packet.RecipientUsername)));
                return;
            }

            recipient.SendPacket(packet);
        }
    }
}
