namespace NebulaModel.Packets.Planet;

public class PlanetDetailResponse
{
    public PlanetDetailResponse() { }

    public PlanetDetailResponse(int planetId, in VeinGroup[] veinGroups, float landPercent)
    {
        PlanetDataID = planetId;
        VeinTypes = new byte[veinGroups.Length];
        VeinCounts = new int[veinGroups.Length];
        VeinAmounts = new long[veinGroups.Length];
        for (var i = 1; i < veinGroups.Length; i++)
        {
            VeinTypes[i] = (byte)veinGroups[i].type;
            VeinCounts[i] = veinGroups[i].count;
            VeinAmounts[i] = veinGroups[i].amount;
        }
        LandPercent = landPercent;
    }

    public int PlanetDataID { get; set; }
    public byte[] VeinTypes { get; set; }
    public int[] VeinCounts { get; set; }
    public long[] VeinAmounts { get; set; }
    public float LandPercent { get; set; }
}
