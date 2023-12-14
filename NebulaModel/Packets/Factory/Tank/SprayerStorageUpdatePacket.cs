namespace NebulaModel.Packets.Factory.Tank;

public class SprayerStorageUpdatePacket
{
    public SprayerStorageUpdatePacket() { }

    public SprayerStorageUpdatePacket(in SpraycoaterComponent spraycoater, int planetId)
    {
        SprayerIndex = spraycoater.id;
        IncItemId = spraycoater.incItemId;
        IncAbility = spraycoater.incAbility;
        IncSprayTimes = spraycoater.incSprayTimes;
        IncCount = spraycoater.incCount;
        ExtraIncCount = spraycoater.extraIncCount;
        PlanetId = planetId;
    }

    public int SprayerIndex { get; }
    public int IncItemId { get; }
    public int IncAbility { get; }
    public int IncSprayTimes { get; }
    public int IncCount { get; }
    public int ExtraIncCount { get; }
    public int PlanetId { get; }
}
