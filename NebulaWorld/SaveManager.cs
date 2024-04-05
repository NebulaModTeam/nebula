#region

using System;
using System.Collections.Generic;
using System.IO;
using NebulaAPI.GameState;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;

#endregion

namespace NebulaWorld;

public static class SaveManager
{
    private const string FILE_EXTENSION = ".server";
    private const ushort REVISION = 8;

    private static readonly Dictionary<string, IPlayerData> playerSaves = new();
    public static IReadOnlyDictionary<string, IPlayerData> PlayerSaves => playerSaves;

    public static void SaveServerData(string saveName)
    {
        var path = GameConfig.gameSaveFolder + saveName + FILE_EXTENSION;
        // var playerManager = Multiplayer.Session.Network.PlayerManager;
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put("REV");
        netDataWriter.Put(REVISION);

        netDataWriter.Put(playerSaves.Count + 1);
        //Add data about all players
        foreach (var data in playerSaves)
        {
            var hash = data.Key;
            netDataWriter.Put(hash);
            data.Value.Serialize(netDataWriter);
        }

        Log.Info(
            $"Saving server data to {saveName + FILE_EXTENSION}, Revision:{REVISION} PlayerCount:{playerSaves.Count}");

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

    public static void LoadServerData(bool loadSaveFile)
    {
        playerSaves.Clear();

        if (!loadSaveFile)
        {
            return;
        }
        var path = GameConfig.gameSaveFolder + DSPGame.LoadFile + FILE_EXTENSION;
        if (!File.Exists(path))
        {
            Log.Info($"No server file");
            return;
        }

        try
        {
            var source = File.ReadAllBytes(path);
            var netDataReader = new NetDataReader(source);
            ushort revision;

            var revString = netDataReader.GetString();
            if (revString != "REV")
            {
                throw new Exception("Incorrect header");
            }

            revision = netDataReader.GetUShort();
            Log.Info($"Loading server data revision {revision} (Latest {REVISION})");
            if (revision != REVISION)
            {
                // Supported revision: 5~8
                if (revision is < 5 or > REVISION)
                {
                    throw new Exception($"Unsupported version {revision}");
                }
            }

            var playerNum = netDataReader.GetInt();


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

                if (!playerSaves.ContainsKey(hash) && playerData != null)
                {
                    playerSaves.Add(hash, playerData);
                }
                else if (playerData == null)
                {
                    Log.Warn($"Could not load player data from unsupported save file revision {revision}");
                }
            }
        }
        catch (Exception e)
        {
            playerSaves.Clear();
            Log.WarnInform("Skipping server data due to exception:\n" + e.Message);
            Log.Warn(e);
            return;
        }
    }

    public static bool TryAdd(string clientCertHash, IPlayerData playerData)
    {
        if (playerSaves.ContainsKey(clientCertHash))
        {
            return false;
        }

        playerSaves.Add(clientCertHash, playerData);
        return true;
    }

    public static bool TryRemove(string clientCertHash)
    {
        return playerSaves.Remove(clientCertHash);
    }
}
