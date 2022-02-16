namespace NebulaModel.Packets.Players
{
    public class PlayerMechaArmor
    {
        public ushort PlayerId { get; set; }
        public byte[] AppearanceData { get; set; }
        public PlayerMechaArmor() { }
        public PlayerMechaArmor(ushort playerId, byte[] appearanceData)
        {
            PlayerId = playerId;
            AppearanceData = appearanceData;
        }
    }
}
