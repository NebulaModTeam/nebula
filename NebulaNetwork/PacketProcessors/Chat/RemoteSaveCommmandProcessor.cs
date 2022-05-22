using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class RemoteSaveCommmandProcessor : PacketProcessor<RemoteSaveCommandPacket>
    {
        DateTime LastSaveTime = DateTime.Now;
        DateTime LastLoginTime = DateTime.Now;
        const int SERVERSAVE_COOLDOWN = 60;
        const int SERVERLOGIN_COOLDOWN = 2;
        readonly HashSet<NebulaConnection> allowedConnections = new();

        public override void ProcessPacket(RemoteSaveCommandPacket packet, NebulaConnection conn)
        {
            string respond = "Unknown command";

            // Don't run remote save command if it is not available
            if (!Config.Options.RemoteAccessEnabled || !Multiplayer.IsDedicated || !IsHost)
            {
                respond = "Remote save access is not enabled on server";
                conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, respond, DateTime.Now, ""));
                return;
            }

            if (!allowedConnections.Contains(conn) && packet.Command != RemoteSaveCommand.Login)
            {
                if (!string.IsNullOrWhiteSpace(Config.Options.RemoteAccessPassword))
                {
                    respond = "You need to login first!";
                    conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, respond, DateTime.Now, ""));
                    return;
                }
                else
                {
                    allowedConnections.Add(conn);
                }
            }

            switch (packet.Command) 
            {
               case RemoteSaveCommand.Login:
                    respond = Login(conn, packet.Content);
                    break;

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

                case RemoteSaveCommand.ServerInfo:
                    Info(conn, packet.Content == "full");
                    return;
            }
            conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, respond, DateTime.Now, ""));
        }

        private string Login(NebulaConnection conn, string passwordHash)
        {
            if (allowedConnections.Contains(conn))
            {
                return "You have already logged in";
            }
            if (!string.IsNullOrWhiteSpace(Config.Options.RemoteAccessPassword))
            {
                int cdtime = SERVERLOGIN_COOLDOWN - (int)(DateTime.Now - LastLoginTime).TotalSeconds;
                if (cdtime > 0)
                {
                    return $"Cooldown: {cdtime}s";
                }
                LastLoginTime = DateTime.Now;
                IPlayerData playerData = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn)?.Data;
                string salt = playerData != null ? playerData.Username + playerData.PlayerId : "";
                string hash = CryptoUtils.Hash(Config.Options.RemoteAccessPassword + salt);
                if (hash != passwordHash)
                {
                    return "Password incorrect!";
                }
            }
            allowedConnections.Add(conn);
            return "Login success!";
        }

        private static string List(string numString)
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
                FileInfo fileInfo = new(files[i]);
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

        private static string Load(string saveName)
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

        private static void Info(NebulaConnection conn, bool full)
        {
            IServer server = Multiplayer.Session.Network as IServer;
            IPUtils.GetIPInfo(server.Port).ContinueWith(async (ipInfo) =>
            {
                string respond = NebulaWorld.Chat.Commands.InfoCommandHandler.GetServerInfoText(server, await ipInfo, full);
                conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, respond, DateTime.Now, ""));
            });
        }
    }
}
