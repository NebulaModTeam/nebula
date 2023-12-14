#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.GameStates;

[RegisterPacketProcessor]
public class FragmentInfoProcessor : PacketProcessor<FragmentInfo>
{
    protected override void ProcessPacket(FragmentInfo packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            GameStatesManager.FragmentSize = packet.Size;
        }
    }
}
