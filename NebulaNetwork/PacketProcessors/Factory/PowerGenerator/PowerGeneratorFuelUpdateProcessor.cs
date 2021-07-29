using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerGenerator;

namespace NebulaNetwork.PacketProcessors.Factory.PowerGenerator
{
    [RegisterPacketProcessor]
    class PowerGeneratorFuelUpdateProcessor : PacketProcessor<PowerGeneratorFuelUpdatePacket>
    {
        public override void ProcessPacket(PowerGeneratorFuelUpdatePacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.genPool;
            if (pool != null && packet.PowerGeneratorIndex != -1 && packet.PowerGeneratorIndex < pool.Length && pool[packet.PowerGeneratorIndex].id != -1)
            {
                if (pool[packet.PowerGeneratorIndex].fuelId != packet.FuelId)
                {
                    pool[packet.PowerGeneratorIndex].SetNewFuel(packet.FuelId, packet.FuelAmount);
                }
                else
                {
                    pool[packet.PowerGeneratorIndex].fuelCount = packet.FuelAmount;
                }
            }
        }
    }
}
