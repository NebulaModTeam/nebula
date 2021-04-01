using LZ4;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Statistics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace NebulaHost
{
    public class StatisticsManager
    {
        public static StatisticsManager instance;
        public ThreadSafeDictionary<ushort, NebulaConnection> Requestors;
        public bool IsStatisticsNeeded;

        private List<StatisticalSnapShot> StatisticalSnapShots;

        public StatisticsManager()
        {
            Requestors = new ThreadSafeDictionary<ushort, NebulaConnection>();
            StatisticalSnapShots = new List<StatisticalSnapShot>();
            IsStatisticsNeeded = false;
            instance = this;
            ClearCapturedData();
        }

        public void ClearCapturedData()
        {
            StatisticalSnapShots.Clear();
        }

        public void CaptureStatisticalSnapshot()
        {
            if (!IsStatisticsNeeded || GameMain.statistics?.production == null)
            {
                return;
            }
            //Calculate number of active factories
            int factoryNum = 0;
            for (ushort i = 0; i < GameMain.statistics.production.factoryStatPool.Length; i++)
            {
                if (GameMain.statistics.production.factoryStatPool[i] != null)
                {
                    factoryNum++;
                } else
                {
                    break;
                }
            }
            StatisticalSnapShot snapshot = new StatisticalSnapShot(GameMain.gameTick, factoryNum);
            FactoryProductionStat stat;
            for (ushort i = 0; i < factoryNum; i++)
            {
                if (GameMain.statistics.production.factoryStatPool[i] != null)
                {
                    stat = GameMain.statistics.production.factoryStatPool[i];
                    //Collect only those that really changed:
                    for (ushort j = 0; j < stat.productRegister.Length; j++)
                    {
                        //Collect production statistics
                        if (stat.productRegister[j] != 0)
                        {
                            snapshot.ProductionChangesPerFactory[i].Add(new ProductionChangeStruct(true, j, stat.productRegister[j]));
                        }

                        //Collect consumption statistics
                        if (stat.consumeRegister[j] != 0)
                        {
                            snapshot.ProductionChangesPerFactory[i].Add(new ProductionChangeStruct(false, j, stat.consumeRegister[j]));
                        }
                    }

                    //Collect Power statistics
                    snapshot.PowerGenRegister[i] = stat.powerGenRegister;
                    snapshot.PowerConRegister[i] = stat.powerConRegister;
                    snapshot.PowerChaRegister[i] = stat.powerChaRegister;
                    snapshot.PowerDisRegister[i] = stat.powerDisRegister;

                    //Collect Energy Stored Values
                    for (int cursor = 0; cursor < GameMain.data.factories[i].powerSystem.netCursor; cursor++) {
                        snapshot.EnergyStored[i] += GameMain.data.factories[i].powerSystem.netPool[cursor].energyStored;
                    }

                    //Collect Research statistics
                    snapshot.HashRegister[i] = stat.hashRegister;
                }
            }
            StatisticalSnapShots.Add(snapshot);
        }

        public void SendBroadcastIfNeeded()
        {
            if (!IsStatisticsNeeded)
            {
                return;
            }
            if (Requestors.Count > 0)
            {
                //Export and prepare update packet
                StatisticUpdateDataPacket updatePacket;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                    using (BufferedStream bs = new BufferedStream(ls, 8192))
                    using (BinaryWriter bw = new BinaryWriter(bs))
                    {
                        ExportCurrentTickData(bw);
                    }
                    updatePacket = new StatisticUpdateDataPacket(ms.ToArray());
                }
                //Broadcast the update packet to the people with opened statistic window
                foreach (var player in Requestors)
                {
                    player.Value.SendPacket(updatePacket);
                }
                ClearCapturedData();
            }
        }

        public void ExportCurrentTickData(BinaryWriter bw)
        {
            bw.Write(StatisticalSnapShots.Count);
            for(int i = 0; i < StatisticalSnapShots.Count; i++)
            {
                StatisticalSnapShots[i].Export(bw);
            }
        }

        public void RegisterPlayer(NebulaConnection nebulaConnection, ushort playerId)
        {
            Requestors.Add(playerId, nebulaConnection);
            if (!IsStatisticsNeeded)
            {
                ClearCapturedData();
                IsStatisticsNeeded = true;
            }
        }

        public void UnRegisterPlayer(ushort playerId)
        {
            if (Requestors.ContainsKey(playerId))
            {
                Requestors.Remove(playerId);
                if (Requestors.Count == 0)
                {
                    IsStatisticsNeeded = false;
                }
            }
        }

        public static void ExportAllData(BinaryWriter bw)
        {
            GameStatData Stats = GameMain.statistics;
            bw.Write(GameMain.data.factoryCount);

            //Export production statistics for every planet
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                Debug.Log("Exporting data for " + i);
                Stats.production.factoryStatPool[i].Export(bw);
            }

            //Export Research statistics
            bw.Write(Stats.techHashedHistory.Length);
            for (int i = 0; i < Stats.techHashedHistory.Length; i++)
            {
                bw.Write(Stats.techHashedHistory[i]);
            }
        }

        public StatisticsPlanetDataPacket GetFactoryPlanetIds()
        {
            int[] result = new int[GameMain.data.factoryCount];
            for(int i = 0; i < result.Length; i++)
            {
                result[i] = GameMain.data.factories[i].planetId;
            }
            return new StatisticsPlanetDataPacket(result);
        }
    }
}
