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
                        string[] energyCapacity = packet.EnergyCapacity[pIndex].Split(';');
                        string[] energyRequired = packet.EnergyRequired[pIndex].Split(';');
                        string[] energyServed = packet.EnergyServed[pIndex].Split(';');
                        string[] energyAccumulated = packet.EnergyAccumulated[pIndex].Split(';');
                        string[] energyExchanged = packet.EnergyExchanged[pIndex].Split(';');
                        string[] consumerRatio = packet.ConsumerRatio[pIndex].Split(';');

                        if(pSys.netCursor != energyCapacity.Length)
                        {
                            return;
                        }

                        for(int j = 0; j < pSys.netCursor; j++)
                        {
                            long capacity, required, served, accumulated, exchanged, ratio;

                            energyCapacity[j].ToLong(out capacity);
                            energyRequired[j].ToLong(out required);
                            energyServed[j].ToLong(out served);
                            energyAccumulated[j].ToLong(out accumulated);
                            energyExchanged[j].ToLong(out exchanged);
                            consumerRatio[j].ToLong(out ratio);

                            pSys.netPool[j].energyCapacity = capacity;
                            pSys.netPool[j].energyRequired = required;
                            pSys.netPool[j].energyServed = served;
                            pSys.netPool[j].energyAccumulated = accumulated;
                            pSys.netPool[j].energyExchanged = exchanged;
                            pSys.netPool[j].consumerRatio = ratio;
                        }
                    }

                    FactoryProductionStat stats = GameMain.statistics.production.factoryStatPool[pData.factory.index];
                    lock (stats)
                    {
                        stats.powerGenRegister = packet.PowerGenRegister[pIndex];
                        stats.powerConRegister = packet.PowerConRegister[pIndex];
                        stats.powerDisRegister = packet.PowerDisRegister[pIndex];
                        stats.powerChaRegister = packet.PowerChaRegister[pIndex];
                        stats.energyConsumption = packet.EnergyConsumption[pIndex];
                    }

                    pIndex++;
                }
            }
        }
    }
}
