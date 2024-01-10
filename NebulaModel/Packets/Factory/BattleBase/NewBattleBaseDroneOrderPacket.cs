namespace NebulaModel.Packets.Factory.BattleBase;

public class NewBattleBaseDroneOrderPacket
{
    public NewBattleBaseDroneOrderPacket() { }

    public NewBattleBaseDroneOrderPacket(int planetId, int entityId, int owner, bool isConstruction)
    {
        PlanetId = planetId;
        EntityId = entityId;
        Owner = owner;
        IsConstruction = isConstruction;
    }

    public int PlanetId { get; set; }
    public int EntityId { get; set; }
    public int Owner { get; set; }
    public bool IsConstruction { get; set; }
}
