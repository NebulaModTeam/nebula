#pragma warning disable IDE1006
namespace NebulaModel.Packets.Universe;

public class DysonSailDataPacket
{
    public DysonSailDataPacket() { }

    public DysonSailDataPacket(int starIndex, ref DysonSail sail, int orbitId, long expiryTime)
    {
        StarIndex = starIndex;
        OrbitId = orbitId;
        ExpiryTime = expiryTime;
        st = sail.st;
        px = sail.px;
        py = sail.py;
        pz = sail.pz;
        vx = sail.vx;
        vy = sail.vy;
        vz = sail.vz;
        gs = sail.gs;
    }

    public int StarIndex { get; set; }
    public int OrbitId { get; set; }
    public long ExpiryTime { get; set; }
    public float st { get; set; }
    public float px { get; set; }
    public float py { get; set; }
    public float pz { get; set; }
    public float vx { get; set; }
    public float vy { get; set; }
    public float vz { get; set; }
    public float gs { get; set; }
}
#pragma warning restore IDE1006
