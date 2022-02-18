﻿using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;
using NebulaWorld;
using System.Collections.Generic;
using System.IO;

namespace NebulaNetwork
{
    public class SaveManager
    {
        private const string FILE_EXTENSION = ".server";
        private const ushort REVISION = 6;

        public static void SaveServerData(string saveName)
        {
            string path = GameConfig.gameSaveFolder + saveName + FILE_EXTENSION;
            IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put("REV");
            netDataWriter.Put(REVISION);

            using (playerManager.GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData))
            {
                netDataWriter.Put(savedPlayerData.Count + 1);
                //Add data about all players
                foreach (KeyValuePair<string, IPlayerData> data in savedPlayerData)
                {
                    string hash = data.Key;
                    netDataWriter.Put(hash);
                    data.Value.Serialize(netDataWriter);
                }
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
            string str1 = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + FILE_EXTENSION;
            string str2 = GameConfig.gameSaveFolder + GameSave.AutoSave0 + FILE_EXTENSION;
            string str3 = GameConfig.gameSaveFolder + GameSave.AutoSave1 + FILE_EXTENSION;
            string str4 = GameConfig.gameSaveFolder + GameSave.AutoSave2 + FILE_EXTENSION;
            string str5 = GameConfig.gameSaveFolder + GameSave.AutoSave3 + FILE_EXTENSION;

            if (File.Exists(str1))
            {
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
        }

        public static void LoadServerData()
        {
            string path = GameConfig.gameSaveFolder + DSPGame.LoadFile + FILE_EXTENSION;

            IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
            if (!File.Exists(path) || playerManager == null)
            {
                return;
            }

            byte[] source = File.ReadAllBytes(path);
            NetDataReader netDataReader = new NetDataReader(source);
            ushort revision = 0;
            try
            {
                string revString = netDataReader.GetString();
                if (revString != "REV")
                {
                    throw new System.Exception();
                }

                revision = netDataReader.GetUShort();
                Log.Info($"Loading server data, revision {revision}");
                if (revision != REVISION)
                {
                    // Supported revision: 4~6
                    if (revision < 4 || revision > REVISION)
                    {
                        throw new System.Exception();
                    }
                }
            }
            catch (System.Exception)
            {
                Log.Warn("Skipping server data from unsupported Nebula version...");
                return;
            }

            int playerNum = netDataReader.GetInt();

            using (playerManager.GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData))
            {
                for (int i = 0; i < playerNum; i++)
                {
                    string hash = netDataReader.GetString();
                    PlayerData playerData = null;
                    if (revision == REVISION)
                    {
                        playerData = netDataReader.Get<PlayerData>();
                    }
                    else if(revision == 4)
                    {
                        playerData = new PlayerData();
                        playerData.Deserialize_4(netDataReader);
                    }
                    else if(revision == 5)
                    {
                        playerData = new PlayerData();
                        playerData.Deserialize_5(netDataReader);
                    }

                    if (!savedPlayerData.ContainsKey(hash) && playerData != null)
                    {
                        savedPlayerData.Add(hash, playerData);
                    }
                    else if(playerData == null)
                    {
                        Log.Warn($"could not load player data from unsupported save file revision {revision}");
                    }
                }
            }

        }
    }
}