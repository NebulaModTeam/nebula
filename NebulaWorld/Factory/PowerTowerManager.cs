#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NebulaModel.Logger;

#endregion

namespace NebulaWorld.Factory;

public class PowerTowerManager : IDisposable
{
    private int ChargerCount;

    private ConcurrentDictionary<int, List<EnergyMapping>> Energy = new();
    public int PlayerChargeAmount;
    private ConcurrentDictionary<int, List<Requested>> RequestsSent = new();

    public void Dispose()
    {
        Energy.Clear();
        Energy = null;

        RequestsSent.Clear();
        RequestsSent = null;

        GC.SuppressFinalize(this);
    }

    public void GivePlayerPower()
    {
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            // host gets it anyways
            return;
        }

        if (PlayerChargeAmount <= 0)
        {
            return;
        }
        GameMain.mainPlayer.mecha.coreEnergy += PlayerChargeAmount;
        GameMain.mainPlayer.mecha.MarkEnergyChange(2, PlayerChargeAmount);
        if (GameMain.mainPlayer.mecha.coreEnergy > GameMain.mainPlayer.mecha.coreEnergyCap)
        {
            GameMain.mainPlayer.mecha.coreEnergy = GameMain.mainPlayer.mecha.coreEnergyCap;
        }
    }

    // return true if added or changed state, false if already known
    public bool AddRequested(int PlanetId, int NetId, int NodeId, bool Charging, bool eventFromOtherPlayer)
    {
        if (RequestsSent.TryGetValue(PlanetId, out var requests))
        {
            foreach (var t in requests.Where(t => t.NetId == NetId && t.NodeId == NodeId))
            {
                if (t.Charging == Charging)
                {
                    return false;
                }
                if (Charging == false)
                {
                    var factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

                    if (factory is { powerSystem: not null })
                    {
                        var baseDemand = factory.powerSystem.nodePool[NodeId].workEnergyPerTick -
                                         factory.powerSystem.nodePool[NodeId].idleEnergyPerTick;
                        var mult = factory.powerSystem.networkServes[NetId];

                        PlayerChargeAmount -= (int)(mult * baseDemand);
                        ChargerCount--;

                        if (PlayerChargeAmount < 0)
                        {
                            PlayerChargeAmount = 0;
                        }
                        if (ChargerCount == 0)
                        {
                            PlayerChargeAmount = 0;
                        }
                    }
                }
                if (!eventFromOtherPlayer)
                {
                    t.Charging = Charging;
                }
                return true;
            }

            var req = new Requested { NetId = NetId, NodeId = NodeId };

            requests.Add(req);

            return true;
        }

        var list = new List<Requested>();
        var req2 = new Requested { NetId = NetId, NodeId = NodeId };

        list.Add(req2);
        return RequestsSent.TryAdd(PlanetId, list);
    }

    public bool DidRequest(int PlanetId, int NetId, int NodeId)
    {
        if (!RequestsSent.TryGetValue(PlanetId, out var requests))
        {
            return false;
        }
        if (!requests.Any(t => t.NetId == NetId && t.NodeId == NodeId && t.Charging))
        {
            return false;
        }
        ChargerCount++;
        return true;
    }

    public int GetExtraDemand(int PlanetId, int NetId)
    {
        if (!Energy.TryGetValue(PlanetId, out var mapping))
        {
            return 0;
        }
        if (Monitor.TryEnter(mapping, 100))
        {
            try
            {
                foreach (var t in mapping.Where(t => t.NetId == NetId))
                {
                    return t.ExtraPower;
                }
            }
            finally
            {
                Monitor.Exit(mapping);
            }
        }
        else
        {
            Log.Warn("PowerTower: cant wait longer for threading lock, PowerTowers will be desynced!");
        }

        return 0;
    }

    public void RemExtraDemand(int PlanetId, int NetId, int NodeId)
    {
        if (!Energy.TryGetValue(PlanetId, out var mapping))
        {
            return;
        }
        foreach (var t in mapping)
        {
            if (t.NetId != NetId)
            {
                continue;
            }
            var factory = GameMain.galaxy.PlanetById(PlanetId).factory;
            var pSystem = factory?.powerSystem;

            if (Monitor.TryEnter(mapping, 100))
            {
                try
                {
                    for (var j = 0; j < t.NodeId.Count; j++)
                    {
                        if (t.NodeId[j] != NodeId)
                        {
                            continue;
                        }
                        if (factory != null && pSystem != null)
                        {
                            t.ExtraPower -= pSystem.nodePool[NodeId].workEnergyPerTick;
                        }
                        else
                        {
                            t.ExtraPower -= t.ExtraPower / t.NodeId.Count;
                        }

                        t.Activated[j]--;
                        AddRequested(PlanetId, NetId, NodeId, false, true);

                        break;
                    }
                }
                finally
                {
                    Monitor.Exit(mapping);
                }
            }
            else
            {
                Log.Warn("PowerTower: cant wait longer for threading lock, PowerTowers will be desynced!");
            }

            if (t.ExtraPower < 0)
            {
                t.ExtraPower = 0;
            }
        }
    }

    public void AddExtraDemand(int PlanetId, int NetId, int NodeId, int PowerAmount)
    {
        while (true)
        {
            if (Energy.TryGetValue(PlanetId, out var mapping))
            {
                foreach (var t in mapping)
                {
                    if (Monitor.TryEnter(mapping, 100))
                    {
                        try
                        {
                            if (t.NetId != NetId)
                            {
                                continue;
                            }
                            var foundNodeId = false;
                            for (var j = 0; j < t.NodeId.Count; j++)
                            {
                                if (t.NodeId[j] != NodeId)
                                {
                                    continue;
                                }
                                foundNodeId = true;
                                t.Activated[j]++;
                                t.ExtraPower += PowerAmount;
                                break;
                            }

                            if (foundNodeId)
                            {
                                return;
                            }
                            t.NodeId.Add(NodeId);
                            t.Activated.Add(1);
                            t.ExtraPower += PowerAmount;

                            return;
                        }
                        finally
                        {
                            Monitor.Exit(mapping);
                        }
                    }
                    Log.Warn("PowerTower: cant wait longer for threading lock, PowerTowers will be desynced!");
                }

                if (Monitor.TryEnter(mapping, 100))
                {
                    try
                    {
                        var map = new EnergyMapping { NetId = NetId };
                        map.NodeId.Add(NodeId);
                        map.Activated.Add(1);
                        map.ExtraPower = PowerAmount;

                        mapping.Add(map);
                    }
                    finally
                    {
                        Monitor.Exit(mapping);
                    }
                }
                else
                {
                    Log.Warn("PowerTower: cant wait longer for threading lock, PowerTowers will be desynced!");
                }
            }
            else
            {
                var mapping2 = new List<EnergyMapping>();

                var map = new EnergyMapping { NetId = NetId };
                map.NodeId.Add(NodeId);
                map.Activated.Add(1);
                map.ExtraPower = PowerAmount;

                mapping2.Add(map);
                if (!Energy.TryAdd(PlanetId, mapping2))
                {
                    // if we failed to add then most likely because another thread was faster, so call this again to run the above part of the method.
                    continue;
                }
            }
            break;
        }
    }

    public void UpdateAllAnimations(int PlanetId)
    {
        if (!Energy.TryGetValue(PlanetId, out var mapping))
        {
            return;
        }
        if (Monitor.TryEnter(mapping, 100))
        {
            try
            {
                foreach (var t in mapping)
                {
                    for (var j = 0; j < t.Activated.Count; j++)
                    {
                        UpdateAnimation(PlanetId, t.NetId, t.NodeId[j],
                            t.Activated[j] > 0 ? 1 : 0);
                    }
                }
            }
            finally
            {
                Monitor.Exit(mapping);
            }
        }
        else
        {
            Log.Warn("PowerTower: cant wait longer for threading lock, PowerTowers will be desynced!");
        }
    }

    private static void UpdateAnimation(int PlanetId, int NetId, int NodeId, int PowerAmount)
    {
        const float idkValue = 0.016666668f;
        var factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

        if (factory is not { entityAnimPool: not null, powerSystem: not null })
        {
            return;
        }
        var pComp = factory.powerSystem.nodePool[NodeId];
        var animPool = factory.entityAnimPool;
        var entityId = pComp.entityId;

        if (pComp.coverRadius < 15f)
        {
            animPool[entityId].StepPoweredClamped(factory.powerSystem.networkServes[NetId], idkValue,
                PowerAmount > 0 ? 2U : 1U);
        }
        else
        {
            animPool[entityId].StepPoweredClamped2(factory.powerSystem.networkServes[NetId], idkValue,
                PowerAmount > 0 ? 2U : 1U);
        }
    }

    private class EnergyMapping
    {
        public readonly List<int> Activated = [];
        public readonly List<int> NodeId = [];
        public int ExtraPower;
        public int NetId;
    }

    private class Requested
    {
        public bool Charging;
        public int NetId;
        public int NodeId;
    }
}
