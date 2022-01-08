using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.PowerSystem;

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
            for(int i = 0; i < GameMain.localStar.planetCount; i++)
            {
                if(GameMain.localStar.planets[i]?.factory != null)
                {
                    PlanetData pData = GameMain.localStar.planets[i];
                    global::PowerSystem pSys = pData.factory?.powerSystem;

                    if(pSys != null && pIndex < packet.EnergyCapacity.Length)
                    {
                        if(pSys.netCursor != packet.EnergyCapacity[pIndex].Length)
                        {
                            return;
                        }

                        for(int j = 0; j < pSys.netCursor; j++)
                        {
                            pSys.netPool[j].energyCapacity = packet.EnergyCapacity[pIndex][j];
                            pSys.netPool[j].energyRequired = packet.EnergyRequired[pIndex][j];
                            pSys.netPool[j].energyServed = packet.EnergyServed[pIndex][j];
                            pSys.netPool[j].energyAccumulated = packet.EnergyAccumulated[pIndex][j];
                            pSys.netPool[j].energyExchanged = packet.EnergyExchanged[pIndex][j];
                            pSys.netPool[j].consumerRatio = packet.ConsumerRatio[pIndex][j];
                        }
                    }

                    FactoryProductionStat stats = GameMain.statistics.production.factoryStatPool[pData.factory.index];
                    if(pIndex < packet.PowerGenRegister.Length)
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

                    pIndex++;
                }
            }
        }
    }
}

