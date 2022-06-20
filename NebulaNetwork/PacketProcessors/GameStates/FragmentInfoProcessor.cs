using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld.GameStates;

namespace NebulaNetwork.PacketProcessors.GameStates
{
    [RegisterPacketProcessor]
    public class FragmentInfoProcessor : PacketProcessor<FragmentInfo>
    {
        public override void ProcessPacket(FragmentInfo packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                GameStatesManager.FragmentSize = packet.Size;
            }
        }
    }
}
