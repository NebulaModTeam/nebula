namespace NebulaModel.Packets.Factory.PowerTower;

public class PowerTowerUserLoadingResponse
{
    public PowerTowerUserLoadingResponse() { }

    public PowerTowerUserLoadingResponse(int PlanetId, int NetId, int NodeId, int PowerAmount, long EnergyCapacity,
        long EnergyRequired, long EnergyServed, long EnergyAccumulated, long EnergyExchanged, bool Charging)
    {
        this.PlanetId = PlanetId;
        this.NetId = NetId;
        this.NodeId = NodeId;
        this.PowerAmount = PowerAmount;
        this.EnergyCapacity = EnergyCapacity;
        this.EnergyRequired = EnergyRequired;
        this.EnergyServed = EnergyServed;
        this.EnergyAccumulated = EnergyAccumulated;
        this.EnergyExchanged = EnergyExchanged;
        this.Charging = Charging;
    }

    public int PlanetId { get; }
    public int NetId { get; }
    public int NodeId { get; }
    public int PowerAmount { get; }
    public long EnergyCapacity { get; }
    public long EnergyRequired { get; }
    public long EnergyServed { get; }
    public long EnergyAccumulated { get; }
    public long EnergyExchanged { get; }
    public bool Charging { get; }
}
