namespace NebulaModel.Packets.Universe
{
    public class DysonBlueprintPacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public EDysonBlueprintType BlueprintType {get; set; }
        public string StringData { get; set; }

        public DysonBlueprintPacket() { }
        public DysonBlueprintPacket(int starIndex, int layerId, EDysonBlueprintType blueprintType, string stringData)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            BlueprintType = blueprintType;
            StringData = stringData;
        }
    }
}
