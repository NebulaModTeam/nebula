#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat.Commands;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class PlayerDataCommmandProcessor : PacketProcessor<PlayerDataCommandPacket>
{
    protected override void ProcessPacket(PlayerDataCommandPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            packet.PlayerData = null;
            var playerSaves = SaveManager.PlayerSaves;
            switch (packet.Command)
            {
                case "list":
                    packet.Message = PlayerDataCommandHandler.GetPlayerDataListString();
                    break;

                case "load":
                    var input = packet.Message;
                    packet.Message = "Unable to find the target player data!";
                    foreach (var pair in playerSaves)
                    {
                        if (input == pair.Key.Substring(0, input.Length) || input == pair.Value.Username)
                        {
                            packet.Message = $"Load [{pair.Key.Substring(0, 5)}] {pair.Value.Username}";
                            packet.PlayerData = (NebulaModel.DataStructures.PlayerData)pair.Value;
                            break;
                        }
                    }
                    break;

                default:
                    packet.Message = "Unknown command: " + packet.Command;
                    break;
            }
            conn.SendPacket(packet);
            return;
        }

        if (IsClient)
        {
            ChatManager.Instance.SendChatMessage(packet.Message, ChatMessageType.CommandOutputMessage);
            if (packet.PlayerData != null)
            {
                PlayerDataCommandHandler.LoadPlayerData(packet.PlayerData);
            }
        }
    }
}
