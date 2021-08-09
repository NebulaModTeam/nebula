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
            NebulaModel.Logger.Log.Info($"Creating {GetType()}");
        }

        public static void ProcessPacket(FactoryData packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()}");

            LocalPlayer.PendingFactories.Add(packet.PlanetId, packet.BinaryData);

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
            }
        }
    }
}
