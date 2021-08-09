using Mirror;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Universe
{
    public struct DysonSphereLoadRequest : NetworkMessage
    {
        public int StarIndex;

        public DysonSphereLoadRequest(int starIndex)
        {
            StarIndex = starIndex;
            NebulaModel.Logger.Log.Info($"Creating {GetType()}");
        }

        public static void ProcessPacket(NetworkConnection conn, DysonSphereLoadRequest packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()}");

            DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                dysonSphere.Export(writer.BinaryWriter);
                conn.Send(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes()));
            }
        }
    }
}
