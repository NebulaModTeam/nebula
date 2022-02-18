﻿namespace NebulaModel.Packets.Players
{
    public class PlayerMechaDIYArmor
    {
        public byte[] DIYAppearanceData { get; set; }
        public int[] DIYItemId { get; set; }
        public int[] DIYItemValue { get; set; }
        public PlayerMechaDIYArmor() { }
        public PlayerMechaDIYArmor(byte[] diyArmorData, int[] diyItemId, int[] diyItemValue)
        {
            DIYAppearanceData = diyArmorData;
            DIYItemId = diyItemId;
            DIYItemValue = diyItemValue;
        }
    }
}
