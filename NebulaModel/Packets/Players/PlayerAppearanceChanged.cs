namespace NebulaModel.Packets.Players
{
    public class PlayerAppearanceChanged
    {
        public ushort PlayerId { get; set; }
        public MechaAppearance Appearance { get; set; }

        public PlayerAppearanceChanged() { }
        public PlayerAppearanceChanged(ushort playerID, byte[] appearance)
        {
            PlayerId = playerID;

            var mechaAppearance = new MechaAppearance();
            mechaAppearance.FromByte(appearance);
            Appearance = mechaAppearance;
        }
    }
}
