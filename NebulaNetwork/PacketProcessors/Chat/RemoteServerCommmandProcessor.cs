#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.Chat.Commands;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class RemoteServerCommmandProcessor : PacketProcessor<RemoteServerCommandPacket>
{
    private const int SERVERSAVE_COOLDOWN = 60;
    private const int SERVERLOGIN_COOLDOWN = 2;
    private readonly HashSet<NebulaConnection> allowedConnections = new();
    private DateTime LastLoginTime = DateTime.Now;
    private DateTime LastSaveTime = DateTime.Now;

    public override void ProcessPacket(RemoteServerCommandPacket packet, NebulaConnection conn)
    {
        var respond = "Unknown command".Translate();

        // Don't run remote save command if it is not available
        if (!Config.Options.RemoteAccessEnabled || !Multiplayer.IsDedicated || !IsHost)
        {
            respond = "Remote server access is not enabled".Translate();
            conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, respond, DateTime.Now, ""));
            return;
        }

        if (!allowedConnections.Contains(conn) && packet.Command != RemoteServerCommand.Login)
        {
            if (!string.IsNullOrWhiteSpace(Config.Options.RemoteAccessPassword))
            {
                respond = "You need to login first!".Translate();
                conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, respond, DateTime.Now, ""));
                return;
            }
            allowedConnections.Add(conn);
        }

        switch (packet.Command)
        {
            case RemoteServerCommand.Login:
                respond = Login(conn, packet.Content);
                break;

            case RemoteServerCommand.ServerList:
                respond = List(packet.Content);
                break;

            case RemoteServerCommand.ServerSave:
                respond = Save(packet.Content);
                break;

            case RemoteServerCommand.ServerLoad:
                respond = Load(packet.Content);
                if (respond == null)
                {
                    // if load success, don't send packet because the connection is reset
                    return;
                }
                break;

            case RemoteServerCommand.ServerInfo:
                Info(conn, packet.Content == "full");
                return;
        }
        conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, respond, DateTime.Now, ""));
    }

    private string Login(NebulaConnection conn, string passwordHash)
    {
        if (allowedConnections.Contains(conn))
        {
            return "You have already logged in".Translate();
        }
        if (!string.IsNullOrWhiteSpace(Config.Options.RemoteAccessPassword))
        {
            var cdtime = SERVERLOGIN_COOLDOWN - (int)(DateTime.Now - LastLoginTime).TotalSeconds;
            if (cdtime > 0)
            {
                return string.Format("Cooldown: {0}s".Translate(), cdtime);
            }
            LastLoginTime = DateTime.Now;
            IPlayerData playerData = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn)?.Data;
            var salt = playerData != null ? playerData.Username + playerData.PlayerId : "";
            var hash = CryptoUtils.Hash(Config.Options.RemoteAccessPassword + salt);
            if (hash != passwordHash)
            {
                return "Password incorrect!".Translate();
            }
        }
        allowedConnections.Add(conn);
        return "Login success!".Translate();
    }

    private static string List(string numString)
    {
        var num = 5;
        if (int.TryParse(numString, out var result))
        {
            num = result;
        }
        // From UILoadGameWindow.RefreshList()
        var files = Directory.GetFiles(GameConfig.gameSaveFolder, "*" + GameSave.saveExt, SearchOption.TopDirectoryOnly);
        var dates = new string[files.Length];
        var names = new string[files.Length];
        num = num < files.Length ? num : files.Length;
        for (var i = 0; i < files.Length; i++)
        {
            FileInfo fileInfo = new(files[i]);
            dates[i] = string.Format("{0:yyyy-MM-dd HH:mm}", fileInfo.LastWriteTime);
            names[i] = fileInfo.Name.Substring(0, fileInfo.Name.Length - GameSave.saveExt.Length);
        }

        Array.Sort(dates, names);
        StringBuilder sb = new();
        sb.AppendLine(string.Format("Save list on server: ({0}/{1})".Translate(), num, files.Length));
        for (var i = files.Length - num; i < files.Length; i++)
        {
            sb.AppendLine($"{dates[i]} {names[i]}");
        }
        return sb.ToString();
    }

    private string Save(string saveName)
    {
        var countDown = SERVERSAVE_COOLDOWN - (int)(DateTime.Now - LastSaveTime).TotalSeconds;
        if (countDown <= 0)
        {
            // Save game and report result to the client
            LastSaveTime = DateTime.Now;
            saveName = string.IsNullOrEmpty(saveName) ? GameSave.LastExit : saveName;
            return string.Format("Save {0} : {1}".Translate(), saveName,
                GameSave.SaveCurrentGame(saveName) ? "Success".Translate() : "Fail".Translate());
        }
        return string.Format("Cooldown: {0}s".Translate(), countDown);
    }

    private static string Load(string saveName)
    {
        if (!GameSave.SaveExist(saveName))
        {
            return string.Format("{0} doesn't exist".Translate(), saveName);
        }

        Log.Info($"Received command to load {saveName}");
        GameStatesManager.ImportedSaveName = saveName;
        UIRoot.instance.uiGame.escMenu.OnButton5Click();
        return null;
    }

    private static void Info(NebulaConnection conn, bool full)
    {
        var server = Multiplayer.Session.Network as IServer;
        IPUtils.GetIPInfo(server.Port).ContinueWith(async ipInfo =>
        {
            var respond = InfoCommandHandler.GetServerInfoText(server, await ipInfo, full);
            conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemInfoMessage, respond, DateTime.Now, ""));
        });
    }
}
