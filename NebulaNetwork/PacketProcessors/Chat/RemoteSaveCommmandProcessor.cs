using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local;
using System;
using System.IO;
using System.Text;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class RemoteSaveCommmandProcessor : PacketProcessor<RemoteSaveCommandPacket>
    {
        DateTime LastSaveTime = DateTime.Now;
        const int SERVERSAVE_COOLDOWN = 60;

        public override void ProcessPacket(RemoteSaveCommandPacket packet, NebulaConnection conn)
        {
            string respond = "Unknown save command";

            // Don't run remote save command if it is not available
            if (!Config.Options.EnableRemoteSaveAccess || !Multiplayer.IsDedicated || !IsHost)
            {
                respond = "Remote save access is not enabled on server";
                conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, respond, DateTime.Now, ""));
                return;
            }

            respond = "Unknown command";
            switch (packet.Command) 
            {
                case RemoteSaveCommand.ServerList:
                    respond = List(packet.Content);
                    break;

                case RemoteSaveCommand.ServerSave:
                    respond = Save(packet.Content);
                    break;

                case RemoteSaveCommand.ServerLoad:
                    respond = Load(packet.Content);
                    if (respond == null)
                    {
                        // if load success, don't send packet because the connection is reset
                        return;
                    }
                    break;
            }
            conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, respond, DateTime.Now, ""));
        }

        private string List(string numString)
        {
            int num = 5;
            if (int.TryParse(numString, out int result))
            {
                num = result;
            }
            // From UILoadGameWindow.RefreshList()
            string[] files = Directory.GetFiles(GameConfig.gameSaveFolder, "*" + GameSave.saveExt, SearchOption.TopDirectoryOnly);
            string[] dates = new string[files.Length];
            string[] names = new string[files.Length];
            num = num < files.Length ? num : files.Length;
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(files[i]);
                dates[i] = string.Format("{0:yyyy-MM-dd HH:mm}", fileInfo.LastWriteTime);
                names[i] = fileInfo.Name.Substring(0, fileInfo.Name.Length - GameSave.saveExt.Length);
            }

            Array.Sort(dates, names);
            StringBuilder sb = new();
            sb.AppendLine($"Save list on server: ({num}/{files.Length})");
            for (int i = files.Length - num; i < files.Length; i++)
            {
                sb.AppendLine($"{dates[i]} {names[i]}");
            }
            return sb.ToString();
        }

        private string Save(string saveName)
        {
            int countDown = SERVERSAVE_COOLDOWN - (int)(DateTime.Now - LastSaveTime).TotalSeconds;
            if (countDown <= 0)
            {
                // Save game and report result to the client
                LastSaveTime = DateTime.Now;
                saveName = string.IsNullOrEmpty(saveName) ? GameSave.LastExit : saveName;
                return $"Save {saveName} : " + (GameSave.SaveCurrentGame(saveName) ? "Success" : "Fail");
            }
            else
            {
                return $"Cooldown: {countDown}s";
            }
        }

        private string Load(string saveName)
        {
            if (!GameSave.SaveExist(saveName))
            {
                return $"{saveName} doesn't exist";
            }

            Log.Info($"Received command to load {saveName}");
            NebulaWorld.GameStates.GameStatesManager.ImportedSaveName = saveName;
            UIRoot.instance.uiGame.escMenu.OnButton5Click();
            return null;
        }
    }
}
