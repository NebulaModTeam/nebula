#region

using System;
using System.Collections.Generic;
using System.IO;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Statistics;
using UnityEngine;
#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE1006 // Naming rule

#endregion

namespace NebulaWorld.Statistics;

public class StatisticsManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();
    private readonly ThreadSafe threadSafe = new();
    private Dictionary<int, int> factoryIndexMap = new();

    private PlanetData[] planetDataMap = new PlanetData[GameMain.data.factories.Length];

    private List<StatisticalSnapShot> statisticalSnapShots = [];
    private long lastUpdateTime;
    private readonly UIReferenceSpeedTip referenceSpeedTip;

    public bool IsStatisticsNeeded { get; set; }
    public long[] PowerEnergyStoredData { get; set; }
    public int FactoryCount { get; set; }
    public int TechHashedFor10Frames { get; set; }

    public StatisticsManager()
    {
        var gameObject = GameObject.Find("UI Root/Overlay Canvas/In Game/Top Tips/Item Reference Speed Tips/ref-speed-tip");
        if (gameObject == null)
        {
            Log.Warn("StatisticsManager: Can't find ref-speed-tip!");
        }
        else
        {
            referenceSpeedTip = gameObject.GetComponent<UIReferenceSpeedTip>();
            if (referenceSpeedTip == null)
            {
                Log.Warn("StatisticsManager: Can't find UIReferenceSpeedTip component!");
            }
        }
    }

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

    public void SendBroadcastIfNeeded(long time)
    {
        if (!IsStatisticsNeeded || time % 60 != 0 || lastUpdateTime == time)
        {
            return;
        }
        lastUpdateTime = time;
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

    public bool HasFactory(PlanetData planet)
    {
        return factoryIndexMap.ContainsKey(planet.id);
    }

    public long UpdateTotalChargedEnergy(int factoryIndex)
    {
        if (PowerEnergyStoredData == null || factoryIndex >= PowerEnergyStoredData.Length) return 0;
        return PowerEnergyStoredData[factoryIndex];
    }

    public void GetReferenceSpeedTip(BinaryWriter bw, int itemId, int astroFilter, int itemCycle, int productionProtoId)
    {
        if (referenceSpeedTip == null) return;

        var tmpItemId = referenceSpeedTip.itemId;
        var tmpAstroFilter = referenceSpeedTip.astroFilter;
        var tmpItemCycle = referenceSpeedTip.itemCycle;
        var tmpProductionProtoId = referenceSpeedTip.productionProtoId;

        referenceSpeedTip.itemId = itemId;
        referenceSpeedTip.astroFilter = astroFilter;
        referenceSpeedTip.itemCycle = (UIReferenceSpeedTip.EItemCycle)itemCycle;
        referenceSpeedTip.productionProtoId = productionProtoId;
        CalculateReferenceSpeedTip();

        var list = new List<int>();
        for (var i = 0; i < referenceSpeedTip.loadedEntryDatas.Length; i++)
        {
            if (referenceSpeedTip.loadedEntryDatas[i].productionProtoId != 0) list.Add(i);
        }
        bw.Write(list.Count);
        foreach (var index in list)
        {
            ref var ptr = ref referenceSpeedTip.loadedEntryDatas[index];
            bw.Write(index);
            bw.Write(ptr.productionProtoId);
            bw.Write(ptr.normalCount);
            bw.Write(ptr.normalSpeed);
            bw.Write(ptr.useInc2IncCount);
            bw.Write(ptr.useInc2IncSpeed);
            bw.Write(ptr.useInc2AccCount);
            bw.Write(ptr.useInc2AccSpeed);
            bw.Write(ptr.outNetworkCount);
            bw.Write(ptr.outNetworkSpeed);
        }

        list.Clear();
        for (var i = 0; i < referenceSpeedTip.loadedSubTipDatas.Length; i++)
        {
            if (referenceSpeedTip.loadedSubTipDatas[i].astroId != 0) list.Add(i);
        }
        bw.Write(list.Count);
        foreach (var index in list)
        {
            ref var ptr = ref referenceSpeedTip.loadedSubTipDatas[index];
            bw.Write(index);
            bw.Write(ptr.astroId);
            bw.Write(ptr.normalCount);
            bw.Write(ptr.normalSpeed);
            bw.Write(ptr.useInc2IncCount);
            bw.Write(ptr.useInc2IncSpeed);
            bw.Write(ptr.useInc2AccCount);
            bw.Write(ptr.useInc2AccSpeed);
            bw.Write(ptr.outNetworkCount);
            bw.Write(ptr.outNetworkSpeed);
        }

        referenceSpeedTip.itemId = tmpItemId;
        referenceSpeedTip.astroFilter = tmpAstroFilter;
        referenceSpeedTip.itemCycle = tmpItemCycle;
        referenceSpeedTip.productionProtoId = tmpProductionProtoId;
        CalculateReferenceSpeedTip();
    }

    public void SetReferenceSpeedTip(BinaryReader br, int itemId, int astroFilter, int itemCycle, int productionProtoId)
    {
        if (referenceSpeedTip == null) return;
        if (referenceSpeedTip.itemId != itemId || referenceSpeedTip.astroFilter != astroFilter || (int)referenceSpeedTip.itemCycle != itemCycle) return;

        Array.Clear(referenceSpeedTip.loadedEntryDatas, 0, referenceSpeedTip.loadedEntryDatas.Length);
        var listCount = br.ReadInt32();
        for (var i = 0; i < listCount; i++)
        {
            var index = br.ReadInt32();
            ref var ptr = ref referenceSpeedTip.loadedEntryDatas[index];
            ptr.productionProtoId = br.ReadInt32();
            ptr.normalCount = br.ReadInt32();
            ptr.normalSpeed = br.ReadSingle();
            ptr.useInc2IncCount = br.ReadInt32();
            ptr.useInc2IncSpeed = br.ReadSingle();
            ptr.useInc2AccCount = br.ReadInt32();
            ptr.useInc2AccSpeed = br.ReadSingle();
            ptr.outNetworkCount = br.ReadInt32();
            ptr.outNetworkSpeed = br.ReadSingle();
        }
        // Refresh UI
        RefreshReferenceSpeedTipEntries();

        if (productionProtoId != 0 && referenceSpeedTip.productionProtoId == productionProtoId)
        {
            Array.Clear(referenceSpeedTip.loadedSubTipDatas, 0, referenceSpeedTip.loadedSubTipDatas.Length);
            listCount = br.ReadInt32();
            for (var i = 0; i < listCount; i++)
            {
                var index = br.ReadInt32();
                ref var ptr = ref referenceSpeedTip.loadedSubTipDatas[index];
                ptr.astroId = br.ReadInt32();
                ptr.normalCount = br.ReadInt32();
                ptr.normalSpeed = br.ReadSingle();
                ptr.useInc2IncCount = br.ReadInt32();
                ptr.useInc2IncSpeed = br.ReadSingle();
                ptr.useInc2AccCount = br.ReadInt32();
                ptr.useInc2AccSpeed = br.ReadSingle();
                ptr.outNetworkCount = br.ReadInt32();
                ptr.outNetworkSpeed = br.ReadSingle();
            }
            referenceSpeedTip.RefreshSubEntries();
        }
    }

    private void CalculateReferenceSpeedTip()
    {
        // Part of UIReferenceSpeedTip.SetSubTip
        if (referenceSpeedTip == null) return;

        if (referenceSpeedTip.loadedEntryDatas == null)
        {
            referenceSpeedTip.loadedEntryDatas = new RefSpeedTipEntryData[12000];
        }
        if (referenceSpeedTip.loadedSubTipDatas == null)
        {
            referenceSpeedTip.loadedSubTipDatas = new RefSpeedSubTipEntryData[25700];
        }
        Array.Clear(referenceSpeedTip.loadedEntryDatas, 0, referenceSpeedTip.loadedEntryDatas.Length);
        Array.Clear(referenceSpeedTip.loadedSubTipDatas, 0, referenceSpeedTip.loadedSubTipDatas.Length);
        var data = GameMain.data;
        var history = data.history;
        var itemProto = LDB.items.Select(2313);
        var array = ((itemProto != null) ? itemProto.prefabDesc.incItemId : null);
        var num = 0;
        if (array != null)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (history.ItemUnlocked(array[i]))
                {
                    var itemProto2 = LDB.items.Select(array[i]);
                    if (itemProto2 != null && itemProto2.Ability > num)
                    {
                        num = itemProto2.Ability;
                    }
                }
            }
        }
        var incMulti = 1f + (float)Cargo.incTableMilli[num];
        var accMulti = 1f + (float)Cargo.accTableMilli[num];
        var maxStack = 1;
        if (history.TechUnlocked(1607))
        {
            maxStack = 4;
        }
        var inserterStackOutput = history.inserterStackOutput;
        var stationPilerLevel = history.stationPilerLevel;
        if (inserterStackOutput > maxStack)
        {
            maxStack = inserterStackOutput;
        }
        if (inserterStackOutput > maxStack)
        {
            maxStack = stationPilerLevel;
        }
        if (referenceSpeedTip.astroFilter == 0)
        {
            var factoryCount = data.factoryCount;
            for (var j = 0; j < factoryCount; j++)
            {
                referenceSpeedTip.AddEntryDataWithFactory(data.factories[j], incMulti, accMulti, maxStack, referenceSpeedTip.productionProtoId);
            }
        }
        else if (referenceSpeedTip.astroFilter % 100 == 0)
        {
            var starData = data.galaxy.StarById(referenceSpeedTip.astroFilter / 100);
            if (starData != null)
            {
                var planets = starData.planets;
                for (var k = 0; k < starData.planetCount; k++)
                {
                    var planetData = planets[k];
                    if (planetData != null && planetData.factory != null)
                    {
                        referenceSpeedTip.AddEntryDataWithFactory(data.factories[planetData.factoryIndex], incMulti, accMulti, maxStack, referenceSpeedTip.productionProtoId);
                    }
                }
            }
            else
            {
                var factoryCount = data.factoryCount;
                for (var l = 0; l < factoryCount; l++)
                {
                    referenceSpeedTip.AddEntryDataWithFactory(data.factories[l], incMulti, accMulti, maxStack, referenceSpeedTip.productionProtoId);
                }
            }
        }
        else
        {
            var planetData = data.galaxy.PlanetById(referenceSpeedTip.astroFilter);
            if (planetData != null && planetData.factory != null)
            {
                referenceSpeedTip.AddEntryDataWithFactory(planetData.factory, incMulti, accMulti, maxStack, referenceSpeedTip.productionProtoId);
            }
        }
    }

    private void RefreshReferenceSpeedTipEntries()
    {
        // Lower part of UIReferenceSpeedTip.SetTip
        if (referenceSpeedTip == null) return;

        ItemProto[] dataArray = LDB.items.dataArray;
        referenceSpeedTip.activeEntryCount = 0;
        float num5 = 0f;
        int num6 = (int)referenceSpeedTip.entryPrefab.rectTrans.anchoredPosition.y;
        int num7 = (int)(referenceSpeedTip.entryPrefab.rectTrans.rect.width + 0.5f);
        for (int m = 0; m < dataArray.Length; m++)
        {
            if (referenceSpeedTip.loadedEntryDatas[dataArray[m].ID].productionProtoId != 0)
            {
                if (referenceSpeedTip.activeEntryCount >= referenceSpeedTip.entries.Count)
                {
                    UIReferenceSpeedTipEntry uireferenceSpeedTipEntry = UnityEngine.Object.Instantiate(referenceSpeedTip.entryPrefab, referenceSpeedTip.entryPrefab.rectTrans.parent);
                    referenceSpeedTip.entries.Add(uireferenceSpeedTipEntry);
                }
                ref RefSpeedTipEntryData ptr = ref referenceSpeedTip.loadedEntryDatas[dataArray[m].ID];
                referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].gameObject.SetActive(true);
                referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].entryData = ptr;
                referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].Refresh();
                referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].rectTrans.anchoredPosition = new Vector2(referenceSpeedTip.entryPrefab.rectTrans.anchoredPosition.x, (float)num6);
                num6 -= referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].entryHeight;
                if (num7 < referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].entryWidth)
                {
                    num7 = referenceSpeedTip.entries[referenceSpeedTip.activeEntryCount].entryWidth;
                }
                num5 += ptr.normalSpeed + ptr.useInc2IncSpeed + ptr.useInc2AccSpeed + ptr.outNetworkSpeed;
                referenceSpeedTip.activeEntryCount++;
            }
        }
        for (int n = referenceSpeedTip.activeEntryCount; n < referenceSpeedTip.entries.Count; n++)
        {
            referenceSpeedTip.entries[n].gameObject.SetActive(false);
        }
        if (referenceSpeedTip.activeEntryCount == 0)
        {
            referenceSpeedTip.zeroCountTipText.gameObject.SetActive(true);
            if (referenceSpeedTip.itemCycle == UIReferenceSpeedTip.EItemCycle.Production)
            {
                referenceSpeedTip.zeroCountTipText.text = "参考速率无生产设施".Translate();
            }
            else if (referenceSpeedTip.itemCycle == UIReferenceSpeedTip.EItemCycle.Consumption)
            {
                referenceSpeedTip.zeroCountTipText.text = "参考速率无消耗设施".Translate();
            }
            num6 -= (int)(referenceSpeedTip.zeroCountTipText.rectTransform.rect.height + 0.5f);
        }
        else
        {
            referenceSpeedTip.zeroCountTipText.gameObject.SetActive(false);
        }
        referenceSpeedTip.totalSpeedText.text = ((long)(num5 + 0.5f)).ToString("#,##0") + " / min";
        if (referenceSpeedTip.itemCycle == UIReferenceSpeedTip.EItemCycle.Production)
        {
            referenceSpeedTip.totalSpeedLabel.color = referenceSpeedTip.productColor;
            referenceSpeedTip.totalSpeedText.color = referenceSpeedTip.productColor;
        }
        else if (referenceSpeedTip.itemCycle == UIReferenceSpeedTip.EItemCycle.Consumption)
        {
            referenceSpeedTip.totalSpeedLabel.color = referenceSpeedTip.consumeColor;
            referenceSpeedTip.totalSpeedText.color = referenceSpeedTip.consumeColor;
        }
        else
        {
            referenceSpeedTip.totalSpeedLabel.color = Color.white;
            referenceSpeedTip.totalSpeedText.color = Color.white;
        }
        referenceSpeedTip.rectTrans.SetParent(UIRoot.instance.itemReferenceSpeedTipTransform, true);
        referenceSpeedTip.rectTrans.sizeDelta = new Vector2(num7 + 20f, (float)(-(float)num6) + 2f);
        Rect rect = UIRoot.instance.itemReferenceSpeedTipTransform.rect;
        float num8 = Mathf.RoundToInt(rect.width);
        float num9 = Mathf.RoundToInt(rect.height);
        Vector2 anchoredPosition = referenceSpeedTip.rectTrans.anchoredPosition;
        float num10 = referenceSpeedTip.rectTrans.anchorMin.x * num8 + anchoredPosition.x;
        float num11 = referenceSpeedTip.rectTrans.anchorMin.y * num9 + anchoredPosition.y;
        Rect rect2 = referenceSpeedTip.rectTrans.rect;
        rect2.x += num10;
        rect2.y += num11;
        Vector2 zero = Vector2.zero;
        if (rect2.xMin < 0f)
        {
            zero.x -= rect2.xMin;
        }
        if (rect2.yMin < 0f)
        {
            zero.y -= rect2.yMin;
        }
        if (rect2.xMax > num8)
        {
            zero.x -= rect2.xMax - num8;
        }
        if (rect2.yMax > num9)
        {
            zero.y -= rect2.yMax - num9;
        }
        Vector2 vector2 = anchoredPosition + zero;
        vector2 = new Vector2(Mathf.Round(vector2.x), Mathf.Round(vector2.y));
        referenceSpeedTip.rectTrans.anchoredPosition = vector2;
    }

    private sealed class ThreadSafe
    {
        internal readonly Dictionary<ushort, NebulaConnection> Requestors = new();
    }
}
