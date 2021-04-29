namespace NebulaModel.Packets.Logistics
{
    public class ILSEnergyConsumeNotification
    {
        public int stationGId { get; set; }
        public long cost { get; set; }
        public ILSEnergyConsumeNotification() { }
        public ILSEnergyConsumeNotification(int stationGId, long cost)
        {
            this.stationGId = stationGId;
            this.cost = cost;
        }
    }
}
