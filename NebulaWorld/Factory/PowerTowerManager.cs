#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NebulaModel.Logger;

#endregion

namespace NebulaWorld.Factory;

public class PowerTowerManager : IDisposable
{
    public int ChargerCount;

    public ConcurrentDictionary<int, List<EnergyMapping>> Energy;
    public int PlayerChargeAmount;
    public ConcurrentDictionary<int, List<Requested>> RequestsSent;

    public PowerTowerManager()
    {
        Energy = new ConcurrentDictionary<int, List<EnergyMapping>>();
        RequestsSent = new ConcurrentDictionary<int, List<Requested>>();
    }

    public void Dispose()
    {
        Energy.Clear();
        Energy = null;

        RequestsSent.Clear();
        RequestsSent = null;
    }

    public void GivePlayerPower()
    {
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            // host gets it anyways
            return;
        }

        if (PlayerChargeAmount > 0)
        {
            GameMain.mainPlayer.mecha.coreEnergy += PlayerChargeAmount;
            GameMain.mainPlayer.mecha.MarkEnergyChange(2, PlayerChargeAmount);
            if (GameMain.mainPlayer.mecha.coreEnergy > GameMain.mainPlayer.mecha.coreEnergyCap)
            {
                GameMain.mainPlayer.mecha.coreEnergy = GameMain.mainPlayer.mecha.coreEnergyCap;
            }
        }
    }

    // return true if added or changed state, false if already known
    public bool AddRequested(int PlanetId, int NetId, int NodeId, bool Charging, bool eventFromOtherPlayer)
    {
        if (RequestsSent.TryGetValue(PlanetId, out var requests))
        {
            for (var i = 0; i < requests.Count; i++)
            {
                if (requests[i].NetId == NetId && requests[i].NodeId == NodeId)
                {
                    if (requests[i].Charging != Charging)
                    {
                        if (Charging == false)
                        {
                            var factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

                            if (factory != null && factory.powerSystem != null)
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
                            requests[i].Charging = Charging;
                        }
                        return true;
                    }
                    return false;
                }
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
        if (RequestsSent.TryGetValue(PlanetId, out var requests))
        {
            for (var i = 0; i < requests.Count; i++)
            {
                if (requests[i].NetId == NetId && requests[i].NodeId == NodeId && requests[i].Charging)
                {
                    ChargerCount++;
                    return true;
                }
            }
        }

        return false;
    }

    public int GetExtraDemand(int PlanetId, int NetId)
    {
        if (Energy.TryGetValue(PlanetId, out var mapping))
        {
            if (Monitor.TryEnter(mapping, 100))
            {
                try
                {
                    for (var i = 0; i < mapping.Count; i++)
                    {
                        if (mapping[i].NetId == NetId)
                        {
                            return mapping[i].ExtraPower;
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

        return 0;
    }

    public void RemExtraDemand(int PlanetId, int NetId, int NodeId)
    {
        if (Energy.TryGetValue(PlanetId, out var mapping))
        {
            for (var i = 0; i < mapping.Count; i++)
            {
                if (mapping[i].NetId == NetId)
                {
                    var factory = GameMain.galaxy.PlanetById(PlanetId).factory;
                    var pSystem = factory?.powerSystem;

                    if (Monitor.TryEnter(mapping, 100))
                    {
                        try
                        {
                            for (var j = 0; j < mapping[i].NodeId.Count; j++)
                            {
                                if (mapping[i].NodeId[j] == NodeId)
                                {
                                    if (factory != null && pSystem != null)
                                    {
                                        mapping[i].ExtraPower -= pSystem.nodePool[NodeId].workEnergyPerTick;
                                    }
                                    else
                                    {
                                        mapping[i].ExtraPower -= mapping[i].ExtraPower / mapping[i].NodeId.Count;
                                    }

                                    mapping[i].Activated[j]--;
                                    AddRequested(PlanetId, NetId, NodeId, false, true);

                                    break;
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

                    if (mapping[i].ExtraPower < 0)
                    {
                        mapping[i].ExtraPower = 0;
                    }
                }
            }
        }
    }

    public void AddExtraDemand(int PlanetId, int NetId, int NodeId, int PowerAmount)
    {
        if (Energy.TryGetValue(PlanetId, out var mapping))
        {
            for (var i = 0; i < mapping.Count; i++)
            {
                if (Monitor.TryEnter(mapping, 100))
                {
                    try
                    {
                        if (mapping[i].NetId == NetId)
                        {
                            var foundNodeId = false;
                            for (var j = 0; j < mapping[i].NodeId.Count; j++)
                            {
                                if (mapping[i].NodeId[j] == NodeId)
                                {
                                    foundNodeId = true;
                                    mapping[i].Activated[j]++;
                                    mapping[i].ExtraPower += PowerAmount;
                                    break;
                                }
                            }

                            if (!foundNodeId)
                            {
                                mapping[i].NodeId.Add(NodeId);
                                mapping[i].Activated.Add(1);
                                mapping[i].ExtraPower += PowerAmount;
                            }

                            return;
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
                AddExtraDemand(PlanetId, NetId, NodeId, PowerAmount);
            }
        }
    }

    public void UpdateAllAnimations(int PlanetId)
    {
        if (Energy.TryGetValue(PlanetId, out var mapping))
        {
            if (Monitor.TryEnter(mapping, 100))
            {
                try
                {
                    for (var i = 0; i < mapping.Count; i++)
                    {
                        for (var j = 0; j < mapping[i].Activated.Count; j++)
                        {
                            UpdateAnimation(PlanetId, mapping[i].NetId, mapping[i].NodeId[j],
                                mapping[i].Activated[j] > 0 ? 1 : 0);
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
    }

    public void UpdateAnimation(int PlanetId, int NetId, int NodeId, int PowerAmount)
    {
        var idkValue = 0.016666668f;
        var factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

        if (factory != null && factory.entityAnimPool != null && factory.powerSystem != null)
        {
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
    }

    public class EnergyMapping
    {
        public List<int> Activated = new();
        public int ExtraPower;
        public int NetId;
        public List<int> NodeId = new();
    }

    public class Requested
    {
        public bool Charging;
        public int NetId;
        public int NodeId;
    }
}
