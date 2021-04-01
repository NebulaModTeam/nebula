namespace NebulaModel.Packets.Statistics
{
    public class StatisticsPlanetDataPacket
    {
        public int[] PlanetsIds { get; set; }

        public StatisticsPlanetDataPacket() { }

        public StatisticsPlanetDataPacket(int[] planetIds)
        {
            this.PlanetsIds = planetIds;
        }
    }
}
