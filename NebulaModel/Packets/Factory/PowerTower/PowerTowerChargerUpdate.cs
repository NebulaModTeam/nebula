namespace NebulaModel.Packets.Factory.PowerTower;

public class PowerTowerChargerUpdate
{
    public PowerTowerChargerUpdate() { }

    public PowerTowerChargerUpdate(int planetId, int nodeId, bool charging)
    {
        PlanetId = planetId;
        NodeId = nodeId;
        Charging = charging;
    }

    public int PlanetId { get; set; }
    public int NodeId { get; set; }
    public bool Charging { get; set; }
}
