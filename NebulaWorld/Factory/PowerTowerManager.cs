using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NebulaWorld.Factory
{
    public static class PowerTowerManager
    {
        public class EnergyMapping
        {
            public int NetId = 0;
            public List<int> NodeId = new List<int>();
            public List<bool> Activated = new List<bool>();
            public int ExtraPower = 0;
        }

        public class Requested
        {
            public int NetId = 0;
            public int NodeId = 0;
            public bool Charging = false;
        }

        public static ConcurrentDictionary<int, List<EnergyMapping>> Energy = new ConcurrentDictionary<int, List<EnergyMapping>>();
        public static ConcurrentDictionary<int, List<Requested>> RequestsSent = new ConcurrentDictionary<int, List<Requested>>();

        public static int ChargerCount = 0;
        public static int PlayerChargeAmount = 0;

        public static void GivePlayerPower()
        {
            if (LocalPlayer.IsMasterClient)
            {
                // host gets it anyways
                return;
            }

            if(PlayerChargeAmount > 0)
            {
                GameMain.mainPlayer.mecha.coreEnergy += (double)PlayerChargeAmount;
                GameMain.mainPlayer.mecha.MarkEnergyChange(2, (double)PlayerChargeAmount);
                if (GameMain.mainPlayer.mecha.coreEnergy > GameMain.mainPlayer.mecha.coreEnergyCap)
                {
                    GameMain.mainPlayer.mecha.coreEnergy = GameMain.mainPlayer.mecha.coreEnergyCap;
                }
            }
        }

        // return true if added or changed state, false if already known
        public static bool AddRequested(int PlanetId, int NetId, int NodeId, bool Charging, int PowerAmount = 0)
        {
            if (RequestsSent.TryGetValue(PlanetId, out var requests))
            {
                for(int i = 0; i < requests.Count; i++)
                {
                    if(requests[i].NetId == NetId && requests[i].NodeId == NodeId)
                    {
                        if(requests[i].Charging != Charging)
                        {
                            if (Charging == false)
                            {
                                PlanetFactory factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

                                if(factory != null && factory.powerSystem != null)
                                {
                                    int baseDemand = factory.powerSystem.nodePool[NodeId].workEnergyPerTick - factory.powerSystem.nodePool[NodeId].idleEnergyPerTick;
                                    float mult = factory.powerSystem.networkServes[NetId];

                                    PlayerChargeAmount -= (int)(mult * (float)baseDemand);
                                    ChargerCount--;

                                    if (PlayerChargeAmount < 0)
                                    {
                                        PlayerChargeAmount = 0;
                                    }
                                    if(ChargerCount == 0)
                                    {
                                        PlayerChargeAmount = 0;
                                    }
                                }
                            }
                            requests[i].Charging = Charging;
                            return true;
                        }
                        return false;
                    }
                }

                Requested req = new Requested();
                req.NetId = NetId;
                req.NodeId = NodeId;

                requests.Add(req);

                return true;
            }

            List<Requested> list = new List<Requested>();
            Requested req2 = new Requested();
            req2.NetId = NetId;
            req2.NodeId = NodeId;

            list.Add(req2);
            return RequestsSent.TryAdd(PlanetId, list);
        }

        public static bool DidRequest(int PlanetId, int NetId, int NodeId)
        {
            if (RequestsSent.TryGetValue(PlanetId, out var requests))
            {
                for (int i = 0; i < requests.Count; i++)
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
        
        public static int GetExtraDemand(int PlanetId, int NetId)
        {

            if(Energy.TryGetValue(PlanetId, out var mapping))
            {
                for(int i = 0; i < mapping.Count; i++)
                {
                    if(mapping[i].NetId == NetId)
                    {
                        return mapping[i].ExtraPower;
                    }
                }
            }

            return 0;
        }

        public static void RemExtraDemand(int PlanetId, int NetId, int NodeId)
        {
            if(Energy.TryGetValue(PlanetId, out var mapping))
            {
                for(int i = 0; i < mapping.Count; i++)
                {
                    if(mapping[i].NetId == NetId)
                    {
                        PlanetFactory factory = GameMain.galaxy.PlanetById(PlanetId).factory;
                        PowerSystem pSystem = factory?.powerSystem;

                        for(int j = 0; j < mapping[i].NodeId.Count; j++)
                        {
                            if(mapping[i].NodeId[j] == NodeId)
                            {
                                if (factory != null && pSystem != null)
                                {
                                    mapping[i].ExtraPower -= pSystem.nodePool[NodeId].workEnergyPerTick;
                                }
                                else
                                {
                                    mapping[i].ExtraPower -= mapping[i].ExtraPower / mapping[i].NodeId.Count;
                                }

                                mapping[i].Activated[j] = false;
                            }
                        }

                        if (mapping[i].ExtraPower < 0)
                        {
                            mapping[i].ExtraPower = 0;
                        }
                    }
                }
            }
        }

        public static void AddExtraDemand(int PlanetId, int NetId, int NodeId, int PowerAmount)
        {
            if (Energy.TryGetValue(PlanetId, out var mapping))
            {
                for(int i = 0; i < mapping.Count; i++)
                {
                    if(mapping[i].NetId == NetId)
                    {
                        bool foundNodeId = false;
                        for(int j = 0; j < mapping[i].NodeId.Count; j++)
                        {
                            if(mapping[i].NodeId[j] == NodeId)
                            {
                                foundNodeId = true;
                                mapping[i].Activated[j] = true;
                                mapping[i].ExtraPower += PowerAmount;
                                break;
                            }
                        }

                        if (!foundNodeId)
                        {
                            mapping[i].NodeId.Add(NodeId);
                            mapping[i].Activated.Add(true);
                            mapping[i].ExtraPower += PowerAmount;
                        }

                        return;
                    }
                }

                EnergyMapping map = new PowerTowerManager.EnergyMapping();
                map.NetId = NetId;
                map.NodeId.Add(NodeId);
                map.Activated.Add(true);
                map.ExtraPower = PowerAmount;

                mapping.Add(map);
            }
            else
            {
                List<EnergyMapping> mapping2 = new List<EnergyMapping>();

                EnergyMapping map = new PowerTowerManager.EnergyMapping();
                map.NetId = NetId;
                map.NodeId.Add(NodeId);
                map.Activated.Add(true);
                map.ExtraPower = PowerAmount;

                mapping2.Add(map);
                if (!Energy.TryAdd(PlanetId, mapping2))
                {
                    // if we failed to add then most likely because another thread was faster, so call this again to run the above part of the method.
                    AddExtraDemand(PlanetId, NetId, NodeId, PowerAmount);
                }
            }
        }

        public static void UpdateAllAnimations(int PlanetId)
        {
            if(Energy.TryGetValue(PlanetId, out var mapping))
            {
                for(int i = 0; i < mapping.Count; i++)
                {
                    for(int j = 0; j < mapping[i].Activated.Count; j++)
                    {
                        UpdateAnimation(PlanetId, mapping[i].NetId, mapping[i].NodeId[j], mapping[i].Activated[j] ? 1 : 0);
                    }
                }
            }
        }

        public static void UpdateAnimation(int PlanetId, int NetId, int NodeId, int PowerAmount)
        {
            float idkValue = 0.016666668f;
            PlanetFactory factory = GameMain.galaxy.PlanetById(PlanetId)?.factory;

            if(factory != null && factory.entityAnimPool != null && factory.powerSystem != null)
            {
                PowerNodeComponent pComp = factory.powerSystem.nodePool[NodeId];
                AnimData[] animPool = factory.entityAnimPool;
                int entityId = pComp.entityId;

                if(pComp.coverRadius < 15f)
                {
                    animPool[entityId].StepPoweredClamped(factory.powerSystem.networkServes[NetId], idkValue, (PowerAmount > 0) ? 2U : 1U);
                }
                else
                {
                    animPool[entityId].StepPoweredClamped2(factory.powerSystem.networkServes[NetId], idkValue, (PowerAmount > 0) ? 2U : 1U);
                }
            }
        }
    }
}
