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

    public int PlanetId { get; set; }
    public int NetId { get; set; }
    public int NodeId { get; set; }
    public int PowerAmount { get; set; }
    public long EnergyCapacity { get; set; }
    public long EnergyRequired { get; set; }
    public long EnergyServed { get; set; }
    public long EnergyAccumulated { get; set; }
    public long EnergyExchanged { get; set; }
    public bool Charging { get; set; }
}
