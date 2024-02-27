namespace NebulaModel.Packets.Trash;

public class TrashSystemLootFilterPacket
{
    public TrashSystemLootFilterPacket() { }

    public TrashSystemLootFilterPacket(byte[] enemyDropBansData)
    {
        EnemyDropBansData = enemyDropBansData;
    }

    public byte[] EnemyDropBansData { get; set; }
}
