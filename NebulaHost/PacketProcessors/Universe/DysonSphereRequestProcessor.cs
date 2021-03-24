using System.IO;
using System.IO.Compression;
using LZ4;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using NebulaModel.Packets.Processors;
using NebulaModel.Logger;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereRequestProcessor : IPacketProcessor<DysonSphereLoadRequest>
    {
        public void ProcessPacket(DysonSphereLoadRequest packet, NebulaConnection conn)
        {
            DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);

            using (MemoryStream ms = new MemoryStream())
            {
                using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                using (BufferedStream bs = new BufferedStream(ls, 8192))
                using (BinaryWriter bw = new BinaryWriter(bs))
                {
                    dysonSphere.Export(bw);
                }
                conn.SendPacket(new DysonSphereData(packet.StarIndex, ms.ToArray()));
            }
        }
    }
}
