namespace NebulaModel.Packets.Combat.DFHive;

public class DFHivePreviewPacket
{
    public DFHivePreviewPacket() { }

    public DFHivePreviewPacket(int hiveAstroId, bool openPreview)
    {
        HiveAstroId = hiveAstroId;
        OpenPreview = openPreview;
    }

    public int HiveAstroId { get; set; }
    public bool OpenPreview { get; set; }
}
