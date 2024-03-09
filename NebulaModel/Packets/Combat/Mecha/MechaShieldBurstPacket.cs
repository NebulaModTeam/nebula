namespace NebulaModel.Packets.Combat.Mecha;

public class MechaShieldBurstPacket
{
    public MechaShieldBurstPacket() { }

    public MechaShieldBurstPacket(ushort playerId, double energyShieldBurstProgress, long energyShieldCapacity,
        long energyShieldEnergy, long energyShieldBurstDamageRate)
    {
        PlayerId = playerId;
        EnergyShieldBurstProgress = energyShieldBurstProgress;
        EnergyShieldCapacity = energyShieldCapacity;
        EnergyShieldEnergy = energyShieldEnergy;
        EnergyShieldBurstDamageRate = energyShieldBurstDamageRate;
    }

    public ushort PlayerId { get; set; }
    public double EnergyShieldBurstProgress { get; set; }
    public long EnergyShieldCapacity { get; set; }
    public long EnergyShieldEnergy { get; set; }
    public long EnergyShieldBurstDamageRate { get; set; }
}
