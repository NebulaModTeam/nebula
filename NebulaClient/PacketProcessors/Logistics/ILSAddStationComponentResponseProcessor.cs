using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using NebulaWorld;
using NebulaWorld.Logistics;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSAddStationComponentResponseProcessor: IPacketProcessor<ILSAddStationComponentResponse>
    {
        public void ProcessPacket(ILSAddStationComponentResponse packet, NebulaConnection conn)
        {
            for(int i = 0; i < ILSShipManager.AddStationComponentQueue[packet.planetId].Count; i++)
            {
                if(ILSShipManager.AddStationComponentQueue[packet.planetId][i].shipDockPos == DataStructureExtensions.ToVector3(packet.shipDockPos))
                {
                    LocalPlayer.PatchLocks["GalacticTransport"] = true;
                    ILSShipManager.AddStationComponentQueue[packet.planetId][i].gid = packet.stationGId;
                    GameMain.data.galacticTransport.AddStationComponent(packet.planetId, ILSShipManager.AddStationComponentQueue[packet.planetId][i]);
                    ILSShipManager.AddStationComponentQueue[packet.planetId].RemoveAt(i);
                    LocalPlayer.PatchLocks["GalacticTransport"] = false;
                    return;
                }
            }
        }
    }
}
