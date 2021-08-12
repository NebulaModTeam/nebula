﻿using NebulaModel.DataStructures;
using Mirror;
using NebulaModel.Packets.Statistics;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NebulaModel.Networking;

namespace NebulaWorld.Statistics
{
    public class StatisticsManager
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<ushort, NetworkConnection> Requestors = new Dictionary<ushort, NetworkConnection>();
        }
        public static readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();
        public static bool IsStatisticsNeeded { get; set; } = false;
        public static long[] PowerEnergyStoredData { get; set; }

        public static StatisticsManager Instance { get; private set; }

        readonly ThreadSafe threadSafe = new ThreadSafe();
        public Locker GetRequestors(out Dictionary<ushort, NetworkConnection> requestors) =>
            threadSafe.Requestors.GetLocked(out requestors);

        private readonly List<StatisticalSnapShot> StatisticalSnapShots;

        public StatisticsManager()
        {
            StatisticalSnapShots = new List<StatisticalSnapShot>();
            IsStatisticsNeeded = false;
            Instance = this;
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
                }
                else
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
            }
            StatisticalSnapShots.Add(snapshot);
        }

        public void SendBroadcastIfNeeded()
        {
            if (!IsStatisticsNeeded)
            {
                return;
            }
            using (Instance.GetRequestors(out var requestors))
            {
                if (requestors.Count > 0)
                {
                    //Export and prepare update packet
                    StatisticUpdateDataPacket updatePacket;
                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        ExportCurrentTickData(writer.BinaryWriter);
                        updatePacket = new StatisticUpdateDataPacket(writer.CloseAndGetBytes());
                    }
                    //Broadcast the update packet to the people with opened statistic window
                    foreach (var player in requestors)
                    {
                        player.Value.SendPacket(updatePacket);
                    }
                    ClearCapturedData();
                }
            }
        }

        public void ExportCurrentTickData(BinaryWriter bw)
        {
            bw.Write(StatisticalSnapShots.Count);
            for (int i = 0; i < StatisticalSnapShots.Count; i++)
            {
                StatisticalSnapShots[i].Export(bw);
            }
        }

        public void RegisterPlayer(NetworkConnection NetworkConnection, ushort playerId)
        {
            using (Instance.GetRequestors(out var requestors))
            {
                requestors.Add(playerId, NetworkConnection);
            }

            if (!IsStatisticsNeeded)
            {
                ClearCapturedData();
                IsStatisticsNeeded = true;
            }
        }

        public static void UnRegisterPlayer(ushort playerId)
        {
            using (Instance.GetRequestors(out var requestors))
            {
                if (requestors.Remove(playerId) && requestors.Count == 0)
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

        public static StatisticsPlanetDataPacket GetFactoryPlanetIds()
        {
            int[] result = new int[GameMain.data.factoryCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GameMain.data.factories[i].planetId;
            }
            return new StatisticsPlanetDataPacket(result);
        }

        public static long UpdateTotalChargedEnergy(int targetIndex)
        {
            long num2 = 0L;

            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return num2;
            }

            //Total Stored Energy for "Entire Star Cluster"
            if (targetIndex == -1)
            {
                for (int i = 0; i < PowerEnergyStoredData.Length; i++)
                {
                    num2 += PowerEnergyStoredData[i];
                }
            }
            //Total Stored Energy for "Local Planet"
            else if (targetIndex == 0)
            {
                num2 = GameMain.data.localPlanet.factoryIndex != -1 ? PowerEnergyStoredData[GameMain.data.localPlanet.factoryIndex] : 0;
            }
            //Total Stored Energy for "Picking specific planet"
            else if (targetIndex % 100 > 0)
            {
                for (int i = 0; i < GameMain.data.factoryCount; i++)
                {
                    if (targetIndex == GameMain.data.factories[i].planetId)
                    {
                        num2 = PowerEnergyStoredData[i];
                        break;
                    }
                }
            }
            //Total Stored Energy for "Picking Star System"
            else if (targetIndex % 100 == 0)
            {
                int starId = targetIndex / 100;
                StarData starData = GameMain.data.galaxy.StarById(starId);
                for (int n = 0; n < starData.planetCount; n++)
                {
                    if (starData.planets[n].factoryIndex != -1)
                    {
                        num2 += PowerEnergyStoredData[starData.planets[n].factoryIndex];
                    }
                }
            }
            return num2;
        }

        public static void ImportAllHistoryData(BinaryReader br)
        {
            GameStatData Stats = GameMain.statistics;

            //Import Factory statistics
            int factoryCount = br.ReadInt32();

            for (int i = 0; i < factoryCount; i++)
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

            //Resfresh the view
            UIRoot.instance.uiGame.statWindow.ComputeDisplayEntries();
        }
    }
}
