namespace NebulaModel.Packets.Players
{
    public class NewDroneOrderPacket
    {
        public int PlanetId { get; set; }
        public int DroneId { get; set; }
        public int EntityId { get; set; }
        public int PlayerId { get; set; }
        public NewDroneOrderPacket() { }
        public NewDroneOrderPacket(int planetId, int droneId, int entityId, int playerId)
        {
            PlanetId = planetId;
            DroneId = droneId;
            EntityId = entityId;
            PlayerId = playerId;
        }
    }
}
