using NebulaModel.Networking;
using NebulaHost.PacketProcessors.Players;
using NebulaModel.Packets;

namespace NebulaClient.PacketProcessors.Players
{
    class RemoveDroneOrdersProcessor : PacketProcessor<RemoveDroneOrdersPacket>
    {
        public override void ProcessPacket(RemoveDroneOrdersPacket packet, NebulaConnection conn)
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
