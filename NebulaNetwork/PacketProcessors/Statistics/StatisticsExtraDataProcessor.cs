#region

using System.IO;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsExtraDataProcessor : PacketProcessor<StatisticsExtraDataPacket>
{
    protected override void ProcessPacket(StatisticsExtraDataPacket packet, NebulaConnection conn)
    {
        using var reader = new BinaryUtils.Reader(packet.BinaryData);
        ImportExtension(reader.BinaryReader, packet.FactoryCount);
    }

    static void ImportExtension(BinaryReader reader, int factoryCount)
    {
        var factoryStatPool = GameMain.data.statistics.production.factoryStatPool;
        for (var i = 0; i < factoryCount; i++)
        {
            var factoryIndex = reader.ReadInt32();
            if (factoryIndex >= factoryStatPool.Length) return; // Abort if factoryProductionStat hasn't imported yet
            var factoryProductionStat = factoryStatPool[factoryIndex];
            ImportProductStatExtension(reader, factoryProductionStat);
        }
    }

    static void ImportProductStatExtension(BinaryReader reader, FactoryProductionStat factoryProductionStat)
    {
        var productCursor = reader.ReadInt32();
        for (var i = 1; i < productCursor; i++)
        {
            var itemId = reader.ReadInt32();
            var refProductSpeed = reader.ReadSingle();
            var refConsumeSpeed = reader.ReadSingle();
            var storageCount = reader.ReadInt64();
            var trashCount = reader.ReadInt64();
            var importStorageCount = reader.ReadInt64();
            var exportStorageCount = reader.ReadInt64();

            if (factoryProductionStat == null) continue;
            var index = factoryProductionStat.productIndices[itemId];
            if (index == 0) continue;

            var product = factoryProductionStat.productPool[index];
            product.refProductSpeed = refProductSpeed;
            product.refConsumeSpeed = refConsumeSpeed;
            product.storageCount = storageCount;
            product.trashCount = trashCount;
            product.importStorageCount = importStorageCount;
            product.exportStorageCount = exportStorageCount;
        }
    }
}
