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

    public int StarIndex { get; }
    public int OrbitId { get; }
    public long ExpiryTime { get; }
    public float st { get; }
    public float px { get; }
    public float py { get; }
    public float pz { get; }
    public float vx { get; }
    public float vy { get; }
    public float vz { get; }
    public float gs { get; }
}
#pragma warning restore IDE1006
