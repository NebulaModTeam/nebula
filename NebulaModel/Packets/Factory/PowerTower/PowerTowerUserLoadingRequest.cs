namespace NebulaModel.Packets.Factory.PowerTower;

public class PowerTowerUserLoadingRequest
{
    public PowerTowerUserLoadingRequest() { }

    public PowerTowerUserLoadingRequest(int PlanetId, int NetId, int NodeId, int PowerAmount, bool Charging)
    {
        this.PlanetId = PlanetId;
        this.NetId = NetId;
        this.NodeId = NodeId;
        this.PowerAmount = PowerAmount;
        this.Charging = Charging;
    }

    public int PlanetId { get; }
    public int NetId { get; }
    public int NodeId { get; }
    public int PowerAmount { get; }
    public bool Charging { get; }
}
