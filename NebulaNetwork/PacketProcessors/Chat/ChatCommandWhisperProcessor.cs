using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Chat.Commands;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class ChatCommandWhisperProcessor : PacketProcessor<ChatCommandWhisperPacket>
    {
        public override void ProcessPacket(ChatCommandWhisperPacket packet, NebulaConnection conn)
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
                INebulaPlayer recipient = Multiplayer.Session.Network
                    .PlayerManager.GetConnectedPlayerByUsername(packet.RecipientUsername);
                if (recipient == null)
                {
                    Log.Warn($"Recipient not found {packet.RecipientUsername}");
                    INebulaPlayer sender = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
                    sender.SendPacket(new ChatCommandWhisperPacket("SYSTEM", packet.SenderUsername, $"User not found {packet.RecipientUsername}"));
                    return;
                }

                recipient.SendPacket(packet);
            }
        }
    }
}