#region

using System.Text;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonBlueprintPacket
{
    public DysonBlueprintPacket() { }

    public DysonBlueprintPacket(int starIndex, int layerId, EDysonBlueprintType blueprintType, string stringData)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        BlueprintType = blueprintType;
        // because string length may exceed maxStringLength in NetSerializer, convert to char array here
        BinaryData = Encoding.ASCII.GetBytes(stringData);
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public EDysonBlueprintType BlueprintType { get; set; }
    public byte[] BinaryData { get; set; }
}
