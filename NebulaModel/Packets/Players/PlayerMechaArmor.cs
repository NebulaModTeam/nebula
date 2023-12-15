namespace NebulaModel.Packets.Players;

public class PlayerMechaArmor
{
    public PlayerMechaArmor() { }

    public PlayerMechaArmor(ushort playerId, byte[] appearanceData)
    {
        PlayerId = playerId;
        AppearanceData = appearanceData;
    }

    public ushort PlayerId { get; set; }
    public byte[] AppearanceData { get; set; }
}
