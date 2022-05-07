using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local;
using System;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class AdminCommandProcessor : PacketProcessor<AdminCommandPacket>
    {
        public override void ProcessPacket(AdminCommandPacket packet, NebulaConnection conn)
        {
            switch (packet.Command) 
            {
                case AdminCommand.ServerSave:
                    if (IsHost && Multiplayer.IsDedicated)
                    {
                        // Save game and report result to clients
                        string saveName = string.IsNullOrEmpty(packet.Content) ? GameSave.LastExit : packet.Content;
                        string message = $"Save {saveName}.dsv : " + (GameSave.SaveCurrentGame(saveName) ? "Success" : "Fail");
                        Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, message, DateTime.Now, "Server"));
                    }
                    break;
            }
        }
    }
}
