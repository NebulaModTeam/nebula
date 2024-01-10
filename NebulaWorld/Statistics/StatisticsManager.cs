#region

using System;
using System.Collections.Generic;
using System.IO;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Statistics;

#endregion

namespace NebulaWorld.Statistics;

public class StatisticsManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();
    private readonly ThreadSafe threadSafe = new();
    private Dictionary<int, int> factoryIndexMap = new();

    private PlanetData[] planetDataMap = new PlanetData[GameMain.data.factories.Length];

    private List<StatisticalSnapShot> statisticalSnapShots = [];

    public bool IsStatisticsNeeded { get; set; }
    public long[] PowerEnergyStoredData { get; set; }
    public int FactoryCount { get; set; }
    public int TechHashedFor10Frames { get; set; }

    public void Dispose()
    {
        statisticalSnapShots = null;
        planetDataMap = null;
        factoryIndexMap = null;
        GC.SuppressFinalize(this);
    }

    private Locker GetRequestors(out Dictionary<ushort, NebulaConnection> requestors)
    {
        return threadSafe.Requestors.GetLocked(out requestors);
    }

    private void ClearCapturedData()
    {
        statisticalSnapShots.Clear();
    }

    public void CaptureStatisticalSnapshot()
    {
        if (!IsStatisticsNeeded || GameMain.statistics?.production == null)
        {
            return;
        }
        var factoryNum = GameMain.data.factoryCount;
        var snapshot = new StatisticalSnapShot(GameMain.gameTick, factoryNum);
        for (ushort i = 0; i < factoryNum; i++)
        {
            var stat = GameMain.statistics.production.factoryStatPool[i];
            //Collect only those that really changed:
            for (ushort j = 0; j < stat.productRegister.Length; j++)
            {
                //Collect production statistics
                if (stat.productRegister[j] != 0)
                {
                    snapshot.ProductionChangesPerFactory[i]
                        .Add(new StatisticalSnapShot.ProductionChangeStruct(true, j, stat.productRegister[j]));
                }

                //Collect consumption statistics
                if (stat.consumeRegister[j] != 0)
                {
                    snapshot.ProductionChangesPerFactory[i]
                        .Add(new StatisticalSnapShot.ProductionChangeStruct(false, j, stat.consumeRegister[j]));
                }
            }

            //Collect Power statistics
            snapshot.PowerGenerationRegister[i] = stat.powerGenRegister;
            snapshot.PowerConsumptionRegister[i] = stat.powerConRegister;
            snapshot.PowerChargingRegister[i] = stat.powerChaRegister;
            snapshot.PowerDischargingRegister[i] = stat.powerDisRegister;

            //Collect Energy Stored Values
            for (var cursor = 0; cursor < GameMain.data.factories[i].powerSystem.netCursor; cursor++)
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
        using (GetRequestors(out var requestors))
        {
            if (requestors.Count <= 0)
            {
                return;
            }
            if (FactoryCount == GameMain.data.factoryCount)
            {
                //Export and prepare update packet
                StatisticUpdateDataPacket updatePacket;
                using (var writer = new BinaryUtils.Writer())
                {
                    ExportCurrentTickData(writer.BinaryWriter);
                    updatePacket = new StatisticUpdateDataPacket(writer.CloseAndGetBytes());
                }
                //Broadcast the update packet to the people with opened statistic window
                foreach (var player in requestors)
                {
                    player.Value.SendPacket(updatePacket);
                }
            }
            else
            {
                //When new planetFactories are added, resend the whole data
                StatisticsDataPacket dataPacket;
                using (var writer = new BinaryUtils.Writer())
                {
                    ExportAllData(writer.BinaryWriter);
                    dataPacket = new StatisticsDataPacket(writer.CloseAndGetBytes());
                }
                foreach (var player in requestors)
                {
                    player.Value.SendPacket(dataPacket);
                }
            }
            ClearCapturedData();
        }
    }

    private void ExportCurrentTickData(BinaryWriter bw)
    {
        bw.Write(statisticalSnapShots.Count);
        foreach (var t in statisticalSnapShots)
        {
            t.Export(bw);
        }
    }

    public void RegisterPlayer(NebulaConnection nebulaConnection, ushort playerId)
    {
        using (GetRequestors(out var requestors))
        {
            requestors.Add(playerId, nebulaConnection);
        }

        if (IsStatisticsNeeded)
        {
            return;
        }
        ClearCapturedData();
        IsStatisticsNeeded = true;
    }

    public void UnRegisterPlayer(ushort playerId)
    {
        using (GetRequestors(out var requestors))
        {
            if (requestors.Remove(playerId) && requestors.Count == 0)
            {
                IsStatisticsNeeded = false;
            }
        }
    }

    public void ExportAllData(BinaryWriter bw)
    {
        var Stats = GameMain.statistics;
        FactoryCount = GameMain.data.factoryCount;
        bw.Write(GameMain.data.factoryCount);

        //Export planetId data
        for (var i = 0; i < GameMain.data.factoryCount; i++)
        {
            bw.Write(GameMain.data.factories[i].planetId);
        }

        //Export production statistics for every planet
        for (var i = 0; i < GameMain.data.factoryCount; i++)
        {
            Stats.production.factoryStatPool[i].Export(bw.BaseStream, bw);
        }

        //Export Research statistics
        bw.Write(Stats.techHashedHistory.Length);
        foreach (var t in Stats.techHashedHistory)
        {
            bw.Write(t);
        }
    }

    public void ImportAllData(BinaryReader br)
    {
        var Stats = GameMain.statistics;
        FactoryCount = br.ReadInt32();

        //Import planet data
        for (var i = 0; i < FactoryCount; i++)
        {
            var pd = GameMain.galaxy.PlanetById(br.ReadInt32());
            if (planetDataMap[i] != null && planetDataMap[i] == pd)
            {
                continue;
            }
            planetDataMap[i] = pd;
            factoryIndexMap[pd.id] = i;
        }

        //Import Factory statistics
        for (var i = 0; i < FactoryCount; i++)
        {
            if (Stats.production.factoryStatPool[i] == null)
            {
                Stats.production.factoryStatPool[i] = new FactoryProductionStat();
                Stats.production.factoryStatPool[i].Init();
            }
            Stats.production.factoryStatPool[i].Import(br.BaseStream, br);
        }

        //Import Research Statistics
        var num = br.ReadInt32();
        if (num > Stats.techHashedHistory.Length)
        {
            Stats.techHashedHistory = new int[num];
        }
        for (var i = 0; i < num; i++)
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
        if (factoryIndexMap.TryGetValue(planet.id, out var factoryIndex))
        {
            return factoryIndex;
        }
        return -1;
    }

    public long UpdateTotalChargedEnergy(int factoryIndex)
    {
        return PowerEnergyStoredData[factoryIndex];
    }

    private sealed class ThreadSafe
    {
        internal readonly Dictionary<ushort, NebulaConnection> Requestors = new();
    }
}
