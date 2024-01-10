#region

using NebulaModel.Networking;

#endregion

namespace NebulaModel.Packets.Trash;

public class TrashSystemNewTrashCreatedPacket
{
    public TrashSystemNewTrashCreatedPacket() { }

    public TrashSystemNewTrashCreatedPacket(int trashId, TrashObject trashObj, TrashData trashData, ushort playerId,
        int localPlanetId)
    {
        TrashId = trashId;
        using (var writer = new BinaryUtils.Writer())
        {
            trashObj.Export(writer.BinaryWriter);
            TrashObjectByte = writer.CloseAndGetBytes();
        }
        using (var writer = new BinaryUtils.Writer())
        {
            trashData.Export(writer.BinaryWriter);
            TrashDataByte = writer.CloseAndGetBytes();
        }
        // Fix overflow in TrashObj.Export() for item.count
        Count = trashObj.count;
        LocalPlanetId = localPlanetId;
        PlayerId = playerId;
    }

    public int TrashId { get; set; }
    public byte[] TrashObjectByte { get; set; }
    public byte[] TrashDataByte { get; set; }
    public int Count { get; set; }
    public ushort PlayerId { get; set; }
    public int LocalPlanetId { get; set; }

    public TrashObject GetTrashObject()
    {
        using var reader = new BinaryUtils.Reader(TrashObjectByte);
        var result = new TrashObject();
        result.Import(reader.BinaryReader);
        result.count = Count;
        return result;
    }

    public TrashData GetTrashData()
    {
        using var reader = new BinaryUtils.Reader(TrashDataByte);
        var result = new TrashData();
        result.Import(reader.BinaryReader);
        return result;
    }
}
