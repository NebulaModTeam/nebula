using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Chat.Commands;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class ChatCommandWhoProcessor : PacketProcessor<ChatCommandWhoPacket>
    {

        public ChatCommandWhoProcessor()
        {
        }

        public override void ProcessPacket(ChatCommandWhoPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                IPlayerData[] playerDatas = Multiplayer.Session.Network.PlayerManager.GetAllPlayerDataIncludingHost();
                ILocalPlayer hostPlayer = Multiplayer.Session.LocalPlayer;
                string resultPayload = WhoCommandHandler.BuildResultPayload(playerDatas, hostPlayer);

                INebulaPlayer recipient = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
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
}