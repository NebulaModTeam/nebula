#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerGenerator;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.PowerGenerator;

[RegisterPacketProcessor]
internal class PowerGeneratorFuelUpdateProcessor : PacketProcessor<PowerGeneratorFuelUpdatePacket>
{
    protected override void ProcessPacket(PowerGeneratorFuelUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.genPool;
        if (pool == null || packet.PowerGeneratorIndex == -1 || packet.PowerGeneratorIndex >= pool.Length ||
            pool[packet.PowerGeneratorIndex].id == -1)
        {
            return;
        }
        if (pool[packet.PowerGeneratorIndex].fuelId != packet.FuelId)
        {
            pool[packet.PowerGeneratorIndex].SetNewFuel(packet.FuelId, packet.FuelAmount, packet.FuelInc);
        }
        else
        {
            pool[packet.PowerGeneratorIndex].fuelCount = packet.FuelAmount;
        }
    }
}
