#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class RemoteOrderUpdateProcessor : PacketProcessor<RemoteOrderUpdate>
{
    protected override void ProcessPacket(RemoteOrderUpdate packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
            var storage = stationComponent?.storage;
            if (stationComponent == null || storage.Length == 0)
            {
                return;
            }
            var remoteOrder = new int[storage.Length];
            for (var i = 0; i < stationComponent.storage.Length; i++)
            {
                remoteOrder[i] = storage[i].remoteOrder;
            }
            packet.RemoteOrder = remoteOrder;
            conn.SendPacket(packet);
        }
        if (!IsClient)
        {
            return;
        }
        {
            var stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
            var storage = stationComponent?.storage;
            if (stationComponent == null || storage.Length == 0)
            {
                return;
            }
            for (var i = 0; i < storage.Length; i++)
            {
                storage[i].remoteOrder = packet.RemoteOrder[i];
            }
        }
    }
}
