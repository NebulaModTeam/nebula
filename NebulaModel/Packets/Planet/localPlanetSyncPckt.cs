namespace NebulaModel.Packets.Planet
{
    public class localPlanetSyncPckt
    {
        public int planetId { get; set; }
        public ushort playerId { get; set; }
        public bool requestUpdate { get; set; } // just to request the current planetId from the other players

        public localPlanetSyncPckt() { }
        public localPlanetSyncPckt(int planetId, bool requestUpdate)
        {
            this.planetId = planetId;
            this.requestUpdate = requestUpdate;
        }
    }
}
