using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NebulaWorld.Statistics
{
    public class StatisticsManager : IDisposable
    {
        private sealed class ThreadSafe
        {
            internal readonly Dictionary<ushort, NebulaConnection> Requestors = new Dictionary<ushort, NebulaConnection>();
        }
        private readonly ThreadSafe threadSafe = new ThreadSafe();

        public Locker GetRequestors(out Dictionary<ushort, NebulaConnection> requestors)
        {
            return threadSafe.Requestors.GetLocked(out requestors);
        }

        private List<StatisticalSnapShot> statisticalSnapShots;

        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();

        public bool IsStatisticsNeeded { get; set; }
        public long[] PowerEnergyStoredData { get; set; }
        public int FactoryCount { get; set; }
        private PlanetData[] planetDataMap;
        private Dictionary<int, int> factoryIndexMap;

        public StatisticsManager()
        {
            statisticalSnapShots = new List<StatisticalSnapShot>();
            planetDataMap = new PlanetData[GameMain.data.factories.Length];
            factoryIndexMap = new Dictionary<int, int>();
            FactoryCount = 0;
        }

        public void Dispose()
        {
            statisticalSnapShots = null;
            planetDataMap = null;
            factoryIndexMap = null;
        }

        public void ClearCapturedData()
        {
            statisticalSnapShots.Clear();
        }

        public void CaptureStatisticalSnapshot()
        {
            if (!IsStatisticsNeeded || GameMain.statistics?.production == null)
            {
                return;
            }
            int factoryNum = GameMain.data.factoryCount;
            StatisticalSnapShot snapshot = new StatisticalSnapShot(GameMain.gameTick, factoryNum);
            FactoryProductionStat stat;
            for (ushort i = 0; i < factoryNum; i++)
            {
                stat = GameMain.statistics.production.factoryStatPool[i];
                //Collect only those that really changed:
                for (ushort j = 0; j < stat.productRegister.Length; j++)
                {
                    //Collect production statistics
                    if (stat.productRegister[j] != 0)
                    {
                        snapshot.ProductionChangesPerFactory[i].Add(new StatisticalSnapShot.ProductionChangeStruct(true, j, stat.productRegister[j]));
                    }

                    //Collect consumption statistics
                    if (stat.consumeRegister[j] != 0)
                    {
                        snapshot.ProductionChangesPerFactory[i].Add(new StatisticalSnapShot.ProductionChangeStruct(false, j, stat.consumeRegister[j]));
                    }
                }

                //Collect Power statistics
                snapshot.PowerGenerationRegister[i] = stat.powerGenRegister;
                snapshot.PowerConsumptionRegister[i] = stat.powerConRegister;
                snapshot.PowerChargingRegister[i] = stat.powerChaRegister;
                snapshot.PowerDischargingRegister[i] = stat.powerDisRegister;

                //Collect Energy Stored Values
                for (int cursor = 0; cursor < GameMain.data.factories[i].powerSystem.netCursor; cursor++)
                {
                    snapshot.EnergyStored[i] += GameMain.data.factories[i].powerSystem.netPool[cursor].energyStored;
                }

                //Collect Research statistics
                snapshot.HashRegister[i] = stat.hashRegister;
            }
            statisticalSnapShots.Add(snapshot);
        }

        public void SendBroadcastIfNeeded()
        {
            if (!IsStatisticsNeeded)
            {
                return;
            }
            using (GetRequestors(out Dictionary<ushort, NebulaConnection> requestors))
            {
                if (requestors.Count > 0)
                {
                    if (FactoryCount == GameMain.data.factoryCount)
                    {
                        //Export and prepare update packet
                        StatisticUpdateDataPacket updatePacket;
                        using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                        {
                            ExportCurrentTickData(writer.BinaryWriter);
                            updatePacket = new StatisticUpdateDataPacket(writer.CloseAndGetBytes());
                        }
                        //Broadcast the update packet to the people with opened statistic window
                        foreach (KeyValuePair<ushort, NebulaConnection> player in requestors)
                        {
                            player.Value.SendPacket(updatePacket);
                        }
                    }
                    else
                    {
                        //When new planetFactories are added, resend the whole data
                        StatisticsDataPacket dataPacket;
                        using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                        {
                            ExportAllData(writer.BinaryWriter);
                            dataPacket = new StatisticsDataPacket(writer.CloseAndGetBytes());
                        }
                        foreach (KeyValuePair<ushort, NebulaConnection> player in requestors)
                        {
                            player.Value.SendPacket(dataPacket);
                        }
                    }
                    ClearCapturedData();
                }
            }
        }

        public void ExportCurrentTickData(BinaryWriter bw)
        {
            bw.Write(statisticalSnapShots.Count);
            for (int i = 0; i < statisticalSnapShots.Count; i++)
            {
                statisticalSnapShots[i].Export(bw);
            }
        }

        public void RegisterPlayer(NebulaConnection nebulaConnection, ushort playerId)
        {
            using (GetRequestors(out Dictionary<ushort, NebulaConnection> requestors))
            {
                requestors.Add(playerId, nebulaConnection);
            }

            if (!IsStatisticsNeeded)
            {
                ClearCapturedData();
                IsStatisticsNeeded = true;
            }
        }

        public void UnRegisterPlayer(ushort playerId)
        {
            using (GetRequestors(out Dictionary<ushort, NebulaConnection> requestors))
            {
                if (requestors.Remove(playerId) && requestors.Count == 0)
                {
                    IsStatisticsNeeded = false;
                }
            }
        }

        public void ExportAllData(BinaryWriter bw)
        {
            GameStatData Stats = GameMain.statistics;
            FactoryCount = GameMain.data.factoryCount;
            bw.Write(GameMain.data.factoryCount);

            //Export planetId data
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                bw.Write(GameMain.data.factories[i].planetId);
            }

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

        public void ImportAllData(BinaryReader br)
        {
            GameStatData Stats = GameMain.statistics;
            FactoryCount = br.ReadInt32();

            //Import planet data
            for (int i = 0; i < FactoryCount; i++)
            {
                PlanetData pd = GameMain.galaxy.PlanetById(br.ReadInt32());
                if (planetDataMap[i] == null || planetDataMap[i] != pd)
                {
                    planetDataMap[i] = pd;
                    factoryIndexMap[pd.id] = i;
                }
            }

            //Import Factory statistics
            for (int i = 0; i < FactoryCount; i++)
            {
                if (Stats.production.factoryStatPool[i] == null)
                {
                    Stats.production.factoryStatPool[i] = new FactoryProductionStat();
                    Stats.production.factoryStatPool[i].Init();
                }
                Stats.production.factoryStatPool[i].Import(br);
            }

            //Import Reserach Statistics
            int num = br.ReadInt32();
            if (num > Stats.techHashedHistory.Length)
            {
                Stats.techHashedHistory = new int[num];
            }
            for (int i = 0; i < num; i++)
            {
                Stats.techHashedHistory[i] = br.ReadInt32();
            }

            //Refresh the view
            UIRoot.instance.uiGame.statWindow.RefreshAll();
        }

        public PlanetData GetPlanetData(int factoryIndex)
        {
            return planetDataMap[factoryIndex];
        }

        public int GetFactoryIndex(PlanetData planet)
        {
            if (factoryIndexMap.TryGetValue(planet.id, out int factoryIndex))
            {
                return factoryIndex;
            }
            else
            {
                return -1;
            }
        }

        public long UpdateTotalChargedEnergy(int factoryIndex)
        {
            return PowerEnergyStoredData[factoryIndex];
        }
    }
}
