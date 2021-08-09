using Mirror;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    public struct FactoryData : NetworkMessage
    {
        public int PlanetId;
        public byte[] BinaryData;

        public FactoryData(int id, byte[] data)
        {
            PlanetId = id;
            BinaryData = data;
            NebulaModel.Logger.Log.Info($"Creating {GetType()} with {BinaryData.Length} bytes");
        }

        public static void ProcessPacket(FactoryData packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()} with {packet.BinaryData.Length} bytes");

            LocalPlayer.PendingFactories.Add(packet.PlanetId, packet.BinaryData);

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
            }
        }
    }
}
