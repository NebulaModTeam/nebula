namespace NebulaModel.Packets.Factory.PowerTower
{
    public class PowerTowerUserLoadingRequest
    {
        public int PlanetId { get; set; }
        public int NetId { get; set; }
        public int NodeId { get; set; }
        public int PowerAmount { get; set; }
        public bool Charging { get; set; }

        public PowerTowerUserLoadingRequest() { }

        public PowerTowerUserLoadingRequest(int PlanetId, int NetId, int NodeId, int PowerAmount, bool Charging)
        {
            this.PlanetId = PlanetId;
            this.NetId = NetId;
            this.NodeId = NodeId;
            this.PowerAmount = PowerAmount;
            this.Charging = Charging;
        }
    }
}
