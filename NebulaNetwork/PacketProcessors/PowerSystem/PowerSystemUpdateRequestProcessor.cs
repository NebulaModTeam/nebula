using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.PowerSystem;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.PowerSystem
{
    [RegisterPacketProcessor]
    public class PowerSystemUpdateRequestProcessor: PacketProcessor<PowerSystemUpdateRequest>
    {
        public override void ProcessPacket(PowerSystemUpdateRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            // is there a better way? There must be
            //long[][] energyCapacity = new long[packet.PlanetIDs.Length][];
            //long[][] energyRequired = new long[packet.PlanetIDs.Length][];
            //long[][] energyServed = new long[packet.PlanetIDs.Length][];
            //long[][] energyAccumulated = new long[packet.PlanetIDs.Length][];
            //long[][] energyExchanged = new long[packet.PlanetIDs.Length][];
            double[][] consumerRatio = new double[packet.PlanetIDs.Length][];
            double[][] generatorRatio = new double[packet.PlanetIDs.Length][];
            bool[][] copyValues = new bool[packet.PlanetIDs.Length][];
            long[][] generateCurrentTick = new long[packet.PlanetIDs.Length][];
            long[][] num35 = new long[packet.PlanetIDs.Length][];
            long[] powerGenRegister = new long[packet.PlanetIDs.Length];
            long[] powerConRegister = new long[packet.PlanetIDs.Length];
            long[] powerDisRegister = new long[packet.PlanetIDs.Length];
            long[] powerChaRegister = new long[packet.PlanetIDs.Length];
            long[] energyConsumption = new long[packet.PlanetIDs.Length];

            for(int i = 0; i < packet.PlanetIDs.Length; i++)
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetIDs[i]);
                global::PowerSystem pSys = pData?.factory?.powerSystem;
                if (pSys != null)
                {
                    //energyCapacity[i] = new long[pSys.netCursor];
                    //energyRequired[i] = new long[pSys.netCursor];
                    //energyServed[i] = new long[pSys.netCursor];
                    //energyAccumulated[i] = new long[pSys.netCursor];
                    //energyExchanged[i] = new long[pSys.netCursor];
                    consumerRatio[i] = new double[pSys.netCursor];
                    generatorRatio[i] = new double[pSys.netCursor];
                    copyValues[i] = new bool[pSys.netCursor];
                    generateCurrentTick[i] = new long[pSys.netCursor];
                    num35[i] = new long[pSys.netCursor];

                    // netPool starts at index 1 but our array starts at index 0 :/
                    for(int j = 0; j < pSys.netCursor - 1; j++)
                    {
                        //energyCapacity[i][j] = pSys.netPool[j].energyCapacity;
                        //energyRequired[i][j] = pSys.netPool[j].energyRequired;
                        //energyServed[i][j] = pSys.netPool[j].energyServed;
                        //energyAccumulated[i][j] = pSys.netPool[j].energyAccumulated;
                        //energyExchanged[i][j] = pSys.netPool[j].energyExchanged;
                        consumerRatio[i][j] = pSys.netPool[j + 1].consumerRatio;
                        generatorRatio[i][j] = pSys.netPool[j + 1].generaterRatio;

                        if(pSys.netPool[j + 1].generators.Count > 0)
                        {
                            generateCurrentTick[i][j] = pSys.genPool[pSys.netPool[j + 1].generators[0]].generateCurrentTick;
                        }
                        else
                        {
                            generateCurrentTick[i][j] = 0;
                        }

                        if (PowerSystemManager.PowerSystemAnimationCache.TryGetValue(pData.id, out var list))
                        {
                            num35[i][j] = j < list.Count ? list[j] : 0;
                        }

                        if((float)pSys.netPool[j + 1].consumerRatio == pSys.networkServes[j + 1])
                        {
                            copyValues[i][j] = true;
                        }
                        else
                        {
                            copyValues[i][j] = false;
                        }
                    }

                    FactoryProductionStat stats = GameMain.statistics.production.factoryStatPool[pData.factory.index];
                    lock (stats)
                    {
                        powerGenRegister[i] = stats.powerGenRegister;
                        powerConRegister[i] = stats.powerConRegister;
                        powerDisRegister[i] = stats.powerDisRegister;
                        powerChaRegister[i] = stats.powerChaRegister;
                        energyConsumption[i] = stats.energyConsumption;
                    }
                }
            }

            conn.SendPacket(new PowerSystemUpdateResponse(consumerRatio, generatorRatio, copyValues, generateCurrentTick, num35, powerGenRegister, powerConRegister, powerDisRegister, powerChaRegister, energyConsumption));
        }
    }
}
