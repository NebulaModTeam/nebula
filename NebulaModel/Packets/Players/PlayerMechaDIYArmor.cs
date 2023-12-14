namespace NebulaModel.Packets.Players;

public class PlayerMechaDIYArmor
{
    public PlayerMechaDIYArmor() { }

    public PlayerMechaDIYArmor(byte[] diyArmorData, int[] diyItemId, int[] diyItemValue)
    {
        DIYAppearanceData = diyArmorData;
        DIYItemId = diyItemId;
        DIYItemValue = diyItemValue;
    }

    public byte[] DIYAppearanceData { get; }
    public int[] DIYItemId { get; }
    public int[] DIYItemValue { get; }
}
