using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;
using NebulaWorld;
using System.Collections.Generic;
using System.IO;

namespace NebulaHost
{
    public class SaveManager
    {
        private const string FILE_EXTENSION = ".server";

        public static void SaveServerData(string saveName)
        {
            string path = GameConfig.gameSaveFolder + saveName + FILE_EXTENSION;
            PlayerManager playerManager = MultiplayerHostSession.Instance.PlayerManager;
            NetDataWriter netDataWriter = new NetDataWriter();

            using (playerManager.GetSavedPlayerData(out var savedPlayerData))
            {
                netDataWriter.Put(savedPlayerData.Count + 1);
                //Add data about all players
                foreach (KeyValuePair<string, PlayerData> data in savedPlayerData)
                {
                    string hash = data.Key;
                    netDataWriter.Put(hash);
                    data.Value.Serialize(netDataWriter);
                }
            }

            //Add host's data
            netDataWriter.Put(CryptoUtils.GetCurrentUserPublicKeyHash());
            LocalPlayer.Data.Serialize(netDataWriter);

            File.WriteAllBytes(path, netDataWriter.Data);

            // If the saveName is the autoSave, we need to rotate the server autosave file.
            if (saveName == GameSave.AutoSaveTmp)
            {
                HandleAutoSave();
            }
        }

        static void HandleAutoSave()
        {
            string str1 = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + FILE_EXTENSION;
            string str2 = GameConfig.gameSaveFolder + GameSave.AutoSave0 + FILE_EXTENSION;
            string str3 = GameConfig.gameSaveFolder + AccessTools.Field(typeof(GameSave), "AutoSave1").GetValue(null) + FILE_EXTENSION;
            string str4 = GameConfig.gameSaveFolder + AccessTools.Field(typeof(GameSave), "AutoSave2").GetValue(null) + FILE_EXTENSION;
            string str5 = GameConfig.gameSaveFolder + AccessTools.Field(typeof(GameSave), "AutoSave3").GetValue(null) + FILE_EXTENSION;

            if (File.Exists(str1))
            {
                if (File.Exists(str5))
                    File.Delete(str5);
                if (File.Exists(str4))
                    File.Move(str4, str5);
                if (File.Exists(str3))
                    File.Move(str3, str4);
                if (File.Exists(str2))
                    File.Move(str2, str3);
                File.Move(str1, str2);
            }
        }

        public static void LoadServerData()
        {
            string path = GameConfig.gameSaveFolder + DSPGame.LoadFile + FILE_EXTENSION;

            PlayerManager playerManager = MultiplayerHostSession.Instance.PlayerManager;
            if (!File.Exists(path) || playerManager == null)
            {
                return;
            }

            byte[] source = File.ReadAllBytes(path);
            NetDataReader netDataReader = new NetDataReader(source);
            int playerNum = netDataReader.GetInt();

            using (playerManager.GetSavedPlayerData(out var savedPlayerData))
            {
                for (int i = 0; i < playerNum; i++)
                {
                    string hash = netDataReader.GetString();
                    PlayerData playerData = netDataReader.Get<PlayerData>();
                    if (!savedPlayerData.ContainsKey(hash))
                    {
                        savedPlayerData.Add(hash, playerData);
                    }
                }
            }

        }
    }
}