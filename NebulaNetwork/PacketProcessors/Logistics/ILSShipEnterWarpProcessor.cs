#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSShipEnterWarpProcessor : PacketProcessor<ILSShipEnterWarp>
{
    public override void ProcessPacket(ILSShipEnterWarp packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            if (packet.ThisGId <= GameMain.data.galacticTransport.stationCursor)
            {
                var stationComponent = GameMain.data.galacticTransport.stationPool[packet.ThisGId];
                if (stationComponent != null && packet.WorkShipIndex < stationComponent.workShipCount)
                {
                    stationComponent.workShipDatas[packet.WorkShipIndex].warpState += 0.016666668f;
                }
            }
        }
    }
}
