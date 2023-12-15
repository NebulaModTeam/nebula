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

    public int SprayerIndex { get; set; }
    public int IncItemId { get; set; }
    public int IncAbility { get; set; }
    public int IncSprayTimes { get; set; }
    public int IncCount { get; set; }
    public int ExtraIncCount { get; set; }
    public int PlanetId { get; set; }
}
