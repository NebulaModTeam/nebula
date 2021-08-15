using NebulaModel.Networking;
using NebulaModel.Packets;

namespace NebulaNetwork.PacketProcessors.Players
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
