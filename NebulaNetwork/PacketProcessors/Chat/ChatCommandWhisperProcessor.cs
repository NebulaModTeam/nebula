#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat.Commands;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class ChatCommandWhisperProcessor : PacketProcessor<ChatCommandWhisperPacket>
{
    protected override void ProcessPacket(ChatCommandWhisperPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            WhisperCommandHandler.SendWhisperToLocalPlayer(packet.SenderUsername, packet.Message);
        }
        else
        {
            // two cases, simplest is that whisper is meant for host
            if (Multiplayer.Session.LocalPlayer.Data.Username == packet.RecipientUsername)
            {
                WhisperCommandHandler.SendWhisperToLocalPlayer(packet.SenderUsername, packet.Message);
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
