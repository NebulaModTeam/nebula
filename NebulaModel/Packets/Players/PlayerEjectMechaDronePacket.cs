namespace NebulaModel.Packets.Players;

public class PlayerEjectMechaDronePacket
{
    public PlayerEjectMechaDronePacket() { }

    public PlayerEjectMechaDronePacket(ushort playerId, int planetId, int targetObjectId,
        int next1ObjectId, int next2ObjectId, int next3ObjectId, int dronePriority)
    {
        PlayerId = playerId;
        PlanetId = planetId;
        TargetObjectId = targetObjectId;
        Next1ObjectId = next1ObjectId;
        Next2ObjectId = next2ObjectId;
        Next3ObjectId = next3ObjectId;
        DronePriority = dronePriority;
    }

    public ushort PlayerId { get; set; }
    public int PlanetId { get; set; }
    public int TargetObjectId { get; set; }
    public int Next1ObjectId { get; set; }
    public int Next2ObjectId { get; set; }
    public int Next3ObjectId { get; set; }
    public int DronePriority { get; set; }
}
