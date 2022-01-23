namespace NebulaModel.Packets.Players
{
    public class PlayerAppearanceChanged
    {
        public ushort PlayerId { get; set; }
        public byte[] Appearance { get; set; }

        public PlayerAppearanceChanged() { }
        public PlayerAppearanceChanged(ushort playerID, MechaAppearance appearance)
        {
            PlayerId = playerID;
            Appearance = appearance.ToByte();
        }
    }
}
