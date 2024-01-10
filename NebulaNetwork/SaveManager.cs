#region

using System;
using System.IO;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;
using NebulaWorld;

#endregion

namespace NebulaNetwork;

public static class SaveManager
{
    private const string FILE_EXTENSION = ".server";
    private const ushort REVISION = 8;

    public static void SaveServerData(string saveName)
    {
        var path = GameConfig.gameSaveFolder + saveName + FILE_EXTENSION;
        var playerManager = Multiplayer.Session.Network.PlayerManager;
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put("REV");
        netDataWriter.Put(REVISION);

        using (playerManager.GetSavedPlayerData(out var savedPlayerData))
        {
            netDataWriter.Put(savedPlayerData.Count + 1);
            //Add data about all players
            foreach (var data in savedPlayerData)
            {
                var hash = data.Key;
                netDataWriter.Put(hash);
                data.Value.Serialize(netDataWriter);
            }
            Log.Info(
                $"Saving server data to {saveName + FILE_EXTENSION}, Revision:{REVISION} PlayerCount:{savedPlayerData.Count}");
        }

        //Add host's data
        netDataWriter.Put(CryptoUtils.GetCurrentUserPublicKeyHash());
        Multiplayer.Session.LocalPlayer.Data.Serialize(netDataWriter);

        File.WriteAllBytes(path, netDataWriter.Data);

        // If the saveName is the autoSave, we need to rotate the server autosave file.
        if (saveName == GameSave.AutoSaveTmp)
        {
            HandleAutoSave();
        }
    }

    private static void HandleAutoSave()
    {
        var str1 = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + FILE_EXTENSION;
        var str2 = GameConfig.gameSaveFolder + GameSave.AutoSave0 + FILE_EXTENSION;
        var str3 = GameConfig.gameSaveFolder + GameSave.AutoSave1 + FILE_EXTENSION;
        var str4 = GameConfig.gameSaveFolder + GameSave.AutoSave2 + FILE_EXTENSION;
        var str5 = GameConfig.gameSaveFolder + GameSave.AutoSave3 + FILE_EXTENSION;

        if (!File.Exists(str1))
        {
            return;
        }
        if (File.Exists(str5))
        {
            File.Delete(str5);
        }

        if (File.Exists(str4))
        {
            File.Move(str4, str5);
        }

        if (File.Exists(str3))
        {
            File.Move(str3, str4);
        }

        if (File.Exists(str2))
        {
            File.Move(str2, str3);
        }

        File.Move(str1, str2);
    }

    public static void LoadServerData()
    {
        var path = GameConfig.gameSaveFolder + DSPGame.LoadFile + FILE_EXTENSION;

        var playerManager = Multiplayer.Session.Network.PlayerManager;
        if (!File.Exists(path) || playerManager == null)
        {
            return;
        }

        var source = File.ReadAllBytes(path);
        var netDataReader = new NetDataReader(source);
        ushort revision;
        try
        {
            var revString = netDataReader.GetString();
            if (revString != "REV")
            {
                throw new Exception();
            }

            revision = netDataReader.GetUShort();
            Log.Info($"Loading server data revision {revision} (Latest {REVISION})");
            if (revision != REVISION)
            {
                // Supported revision: 5~8
                if (revision is < 5 or > REVISION)
                {
                    throw new Exception();
                }
            }
        }
        catch (Exception)
        {
            Log.Warn("Skipping server data from unsupported Nebula version...");
            return;
        }

        var playerNum = netDataReader.GetInt();

        using (playerManager.GetSavedPlayerData(out var savedPlayerData))
        {
            for (var i = 0; i < playerNum; i++)
            {
                var hash = netDataReader.GetString();
                PlayerData playerData = null;
                switch (revision)
                {
                    case REVISION:
                        playerData = netDataReader.Get(() => new PlayerData());
                        break;
                    case >= 5:
                        playerData = new PlayerData();
                        playerData.Import(netDataReader, revision);
                        break;
                }

                if (!savedPlayerData.ContainsKey(hash) && playerData != null)
                {
                    savedPlayerData.Add(hash, playerData);
                }
                else if (playerData == null)
                {
                    Log.Warn($"could not load player data from unsupported save file revision {revision}");
                }
            }
        }
    }
}
