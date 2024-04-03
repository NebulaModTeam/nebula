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
public class ILSIdleShipBackToWorkProcessor : PacketProcessor<ILSIdleShipBackToWork>
{
    protected override void ProcessPacket(ILSIdleShipBackToWork packet, NebulaConnection conn)
    {
        if (!IsClient)
        {
            return;
        }

        try
        {
            ILSShipManager.IdleShipGetToWork(packet);
        }
        catch (Exception e)
        {
            Log.Warn(e);
        }
    }
}
