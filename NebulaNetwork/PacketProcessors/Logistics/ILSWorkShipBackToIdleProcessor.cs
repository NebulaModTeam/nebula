#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSWorkShipBackToIdleProcessor : PacketProcessor<ILSWorkShipBackToIdle>
{
    protected override void ProcessPacket(ILSWorkShipBackToIdle packet, NebulaConnection conn)
    {
        if (!IsClient)
        {
            return;
        }

        try
        {
            ILSShipManager.WorkShipBackToIdle(packet);
        }
        catch (Exception e)
        {
            Log.Warn(e);
        }
    }
}
