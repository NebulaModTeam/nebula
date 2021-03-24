using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using NebulaModel.Packets.Processors;
using System.IO;
using LZ4;
using System.IO.Compression;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereDataProcessor : IPacketProcessor<DysonSphereData>
    {
        public void ProcessPacket(DysonSphereData packet, NebulaConnection conn)
        {
            if (GameMain.data.dysonSpheres[packet.StarIndex] == null)
            {
                GameMain.data.dysonSpheres[packet.StarIndex] = new DysonSphere();
                GameMain.data.statistics.production.Init(GameMain.data);
                GameMain.data.statistics.production.factoryStatPool[0] = new FactoryProductionStat();
                GameMain.data.statistics.production.factoryStatPool[0].Init();
                GameMain.data.dysonSpheres[packet.StarIndex].Init(GameMain.data, GameMain.data.galaxy.stars[packet.StarIndex]);
            }

            using (MemoryStream ms = new MemoryStream(packet.BinaryData))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                GameMain.data.dysonSpheres[packet.StarIndex].Import(br);
            }
        }
    }
}
