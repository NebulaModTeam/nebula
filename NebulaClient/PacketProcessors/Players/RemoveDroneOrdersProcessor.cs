using NebulaModel.Networking;
using NebulaHost.PacketProcessors.Players;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Players
{
    class RemoveDroneOrdersProcessor : IPacketProcessor<RemoveDroneOrdersPacket>
    {
        public void ProcessPacket(RemoveDroneOrdersPacket packet, NebulaConnection conn)
        {
            if (packet.QueuedEntityIds != null)
            {
                for (int i = 0; i < packet.QueuedEntityIds.Length; i++)
                {
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(packet.QueuedEntityIds[i]);
                }
            }
        }
    }
}
