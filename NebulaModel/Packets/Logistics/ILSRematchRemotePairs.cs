#region

using System.Collections.Generic;

#endregion

namespace NebulaModel.Packets.Logistics;

public class ILSRematchRemotePairs
{
    public ILSRematchRemotePairs() { }

    public ILSRematchRemotePairs(int gid, List<int> shipIndex, List<int> otherGId, List<int> direction, List<int> itemId)
    {
        GId = gid;
        ShipIndex = shipIndex.ToArray();
        OtherGId = otherGId.ToArray();
        Direction = direction.ToArray();
        ItemId = itemId.ToArray();
    }

    public int GId { get; }
    public int[] ShipIndex { get; }
    public int[] OtherGId { get; }
    public int[] Direction { get; }
    public int[] ItemId { get; }
}
