using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.PowerSystem;

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
            string[] energyCapacity = new string[packet.PlanetIDs.Length];
            string[] energyRequired = new string[packet.PlanetIDs.Length];
            string[] energyServed = new string[packet.PlanetIDs.Length];
            string[] energyAccumulated = new string[packet.PlanetIDs.Length];
            string[] energyExchanged = new string[packet.PlanetIDs.Length];
            string[] consumerRatio = new string[packet.PlanetIDs.Length];
            long[] powerGenRegister = new long[packet.PlanetIDs.Length];
            long[] powerConRegister = new long[packet.PlanetIDs.Length];
            long[] powerDisRegister = new long[packet.PlanetIDs.Length];
            long[] powerChaRegister = new long[packet.PlanetIDs.Length];
            long[] energyConsumption = new long[packet.PlanetIDs.Length];

            for(int i = 0; i < packet.PlanetIDs.Length; i++)
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetIDs[i]);
                global::PowerSystem pSys = pData.factory?.powerSystem;
                if (pSys != null)
                {
                    for(int j = 0; j < pSys.netCursor; j++)
                    {
                        if(j == 0)
                        {
                            energyCapacity[i] = pSys.netPool[j].energyCapacity.ToString();
                            energyRequired[i] = pSys.netPool[j].energyRequired.ToString();
                            energyServed[i] = pSys.netPool[j].energyServed.ToString();
                            energyAccumulated[i] = pSys.netPool[j].energyAccumulated.ToString();
                            energyExchanged[i] = pSys.netPool[j].energyExchanged.ToString();
                            consumerRatio[i] = pSys.netPool[j].consumerRatio.ToString();
                        }
                        else
                        {
                            energyCapacity[i] += ";" + pSys.netPool[j].energyCapacity.ToString();
                            energyRequired[i] += ";" + pSys.netPool[j].energyRequired.ToString();
                            energyServed[i] += ";" + pSys.netPool[j].energyServed.ToString();
                            energyAccumulated[i] += ";" + pSys.netPool[j].energyAccumulated.ToString();
                            energyExchanged[i] += ";" + pSys.netPool[j].energyExchanged.ToString();
                            consumerRatio[i] += ";" + pSys.netPool[j].consumerRatio.ToString();
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

            conn.SendPacket(new PowerSystemUpdateResponse(energyCapacity, energyRequired, energyServed, energyAccumulated, energyExchanged, consumerRatio, powerGenRegister, powerConRegister, powerDisRegister, powerChaRegister, energyConsumption));
        }
    }
}
