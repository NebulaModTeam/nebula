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
        private static string lastFileName = "";

        public static bool SaveOnExit = false;
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
        }

        public static void SetLastSave(string fileName)
        {
            lastFileName = fileName;
        }

        public static void LoadServerData()
        {
            string path = GameConfig.gameSaveFolder + lastFileName + FILE_EXTENSION;
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