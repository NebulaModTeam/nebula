#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSShipEnterWarpProcessor : PacketProcessor<ILSShipEnterWarp>
{
    protected override void ProcessPacket(ILSShipEnterWarp packet, NebulaConnection conn)
    {
        if (!IsClient)
        {
            return;
        }
        if (packet.ThisGId > GameMain.data.galacticTransport.stationCursor)
        {
            return;
        }
        var stationComponent = GameMain.data.galacticTransport.stationPool[packet.ThisGId];
        if (stationComponent != null && packet.WorkShipIndex < stationComponent.workShipCount)
        {
            stationComponent.workShipDatas[packet.WorkShipIndex].warpState += 0.016666668f;
        }
    }
}
