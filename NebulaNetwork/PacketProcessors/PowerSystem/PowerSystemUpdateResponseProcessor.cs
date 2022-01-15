using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.PowerSystem;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.PowerSystem
{
    [RegisterPacketProcessor]
    public class PowerSystemUpdateResponseProcessor: PacketProcessor<PowerSystemUpdateResponse>
    {
        public override void ProcessPacket(PowerSystemUpdateResponse packet, NebulaConnection conn)
        {
            if (IsHost || GameMain.localStar == null)
            {
                return;
            }

            int pIndex = 0;

            for(int i = 0; i < GameMain.localStar?.planetCount; i++)
            {
                if(GameMain.localStar.planets[i]?.factory != null)
                {
                    PlanetData pData = GameMain.localStar.planets[i];
                    global::PowerSystem pSys = pData.factory?.powerSystem;

                    Log.Warn($"cl: {packet.ConsumerRatio.Length} ps on {pData.displayName} is {pSys != null} and factory is {pData.factory != null} ({i})");

                    if (pSys != null && pIndex < packet.ConsumerRatio.Length)
                    {
                        if(pSys.netCursor != packet.ConsumerRatio[pIndex].Length)
                        {
                            Log.Warn($"nc: {pSys.netCursor} len: {packet.ConsumerRatio[pIndex].Length}");
                            return;
                        }

                        Log.Warn($"nc: {pSys.netCursor}");

                        // netPool starts at index 1 but our array starts at index 0 :/
                        for (int j = 0; j < pSys.netCursor - 1; j++)
                        {
                            //pSys.netPool[j].energyCapacity = packet.EnergyCapacity[pIndex][j];
                            //pSys.netPool[j].energyRequired = packet.EnergyRequired[pIndex][j];
                            //pSys.netPool[j].energyServed = packet.EnergyServed[pIndex][j];
                            //pSys.netPool[j].energyAccumulated = packet.EnergyAccumulated[pIndex][j];
                            //pSys.netPool[j].energyExchanged = packet.EnergyExchanged[pIndex][j];
                            pSys.netPool[j + 1].consumerRatio = packet.ConsumerRatio[pIndex][j];
                            pSys.netPool[j + 1].generaterRatio = packet.GeneratorRatio[pIndex][j];
                            if (packet.CopyValues[pIndex][j])
                            {
                                pSys.networkServes[j] = (float)packet.ConsumerRatio[pIndex][j];
                                pSys.networkGenerates[j] = (float)packet.GeneratorRatio[pIndex][j];
                            }
                            else
                            {
                                pSys.networkServes[j] = 0f;
                                pSys.networkGenerates[j] = 0f;
                            }

                            /*
                            for(int k = 0; k < pSys.netPool[j + 1].generators.Count; k++)
                            {
                                pSys.genPool[pSys.netPool[j + 1].generators[k]].generateCurrentTick = packet.GenerateCurrentTick[pIndex][j];
                            }
                            */
                            
                            if (PowerSystemManager.PowerSystemAnimationCache.TryGetValue(pData.id, out var list)){
                                if (j < list.Count)
                                {
                                    Log.Info($"adding {packet.Num35[pIndex][j]} to cache");
                                    list[j] = packet.Num35[pIndex][j];
                                }
                                else
                                {
                                    Log.Info("new list entry");
                                    list.Add(packet.Num35[pIndex][j]);
                                }
                            }
                            else
                            {
                                List<long> newList = new List<long>();
                                newList.Add(packet.Num35[pIndex][j]);

                                PowerSystemManager.PowerSystemAnimationCache.TryAdd(pData.id, newList);
                            }
                        }

                        pIndex++;
                    }

                    /*
                    FactoryProductionStat stats = GameMain.statistics.production.factoryStatPool[pData.factory.index];
                    if(i < packet.PowerGenRegister.Length)
                    {
                        lock (stats)
                        {
                            stats.powerGenRegister = packet.PowerGenRegister[pIndex];
                            stats.powerConRegister = packet.PowerConRegister[pIndex];
                            stats.powerDisRegister = packet.PowerDisRegister[pIndex];
                            stats.powerChaRegister = packet.PowerChaRegister[pIndex];
                            stats.energyConsumption = packet.EnergyConsumption[pIndex];
                        }
                    }
                    */
                }
            }
        }
    }
}

