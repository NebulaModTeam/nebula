﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class RemoteOrderUpdateProcessor : PacketProcessor<RemoteOrderUpdate>
    {
        public override void ProcessPacket(RemoteOrderUpdate packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
                StationStore[] storage = stationComponent?.storage;
                if (stationComponent == null || storage == null)
                {
                    return;
                }
                int[] remoteOrder = new int[storage.Length];
                for (int i = 0; i < stationComponent.storage.Length; i++)
                {
                    remoteOrder[i] = storage[i].remoteOrder;
                }
                packet.RemoteOrder = remoteOrder;
                conn.SendPacket(packet);
            }
            if (IsClient)
            {
                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
                StationStore[] storage = stationComponent?.storage;
                if (stationComponent == null || storage == null)
                {
                    return;
                }
                for (int i = 0; i < storage.Length; i++)
                {
                    storage[i].remoteOrder = packet.RemoteOrder[i];
                }
            }
        }
    }
}