namespace NebulaModel.Packets.Players;

public class PlayerReplicatorMultipliersPacket
{
    public PlayerReplicatorMultipliersPacket() { }

    public PlayerReplicatorMultipliersPacket(ushort playerId, byte[] multipliersData)
    {
        PlayerId = playerId;
        MultipliersData = multipliersData;
    }

    public ushort PlayerId { get; set; }
    public byte[] MultipliersData { get; set; }
}
