#region

#endregion

namespace NebulaModel.Packets.Players;

public class NewMechaDroneOrderPacket
{
    public NewMechaDroneOrderPacket() { }

    public NewMechaDroneOrderPacket(int planetId, int entityId, ushort playerId, bool priority)
    {
        PlanetId = planetId;
        EntityId = entityId;
        PlayerId = playerId;
        Priority = priority;
    }

    public int PlanetId { get; set; }
    public int EntityId { get; set; }
    public ushort PlayerId { get; set; }
    public bool Priority { get; set; }
}
