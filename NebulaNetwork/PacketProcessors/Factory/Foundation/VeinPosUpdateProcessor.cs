#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Foundation;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Foundation;

[RegisterPacketProcessor]
internal class VeinPosUpdateProcessor : PacketProcessor<VeinPosUpdatePacket>
{
    protected override void ProcessPacket(VeinPosUpdatePacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        if (planet?.factory == null)
        {
            return;
        }

        var pos = packet.Pos.ToVector3();
        ref var veinData = ref planet.factory.veinPool[packet.VeinId];
        veinData.pos = pos;
        if (planet != GameMain.localPlanet)
        {
            return;
        }

        // Update GPU models on the local planet
        var rot = Maths.SphericalRotation(pos, Random.value * 360f);
        GameMain.gpuiManager.AlterModel(veinData.modelIndex, veinData.modelId, packet.VeinId, pos, rot, false);
        var veinProto = LDB.veins.Select((int)veinData.type);
        if (veinProto == null)
        {
            return;
        }
        var magnitude = pos.magnitude;
        var normalVector = pos / magnitude;
        if (veinData.minerId0 > 0)
        {
            GameMain.gpuiManager.AlterModel(veinProto.MinerBaseModelIndex, veinData.minerBaseModelId, veinData.minerId0, normalVector * (magnitude + 0.1f), false);
            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinData.minerCircleModelId0, veinData.minerId0, normalVector * (magnitude + 0.4f), false);
        }
        if (veinData.minerId1 > 0)
        {
            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinData.minerCircleModelId1, veinData.minerId1, normalVector * (magnitude + 0.6f), false);
        }
        if (veinData.minerId2 > 0)
        {
            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinData.minerCircleModelId2, veinData.minerId2, normalVector * (magnitude + 0.8f), false);
        }
        if (veinData.minerId3 > 0)
        {
            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinData.minerCircleModelId3, veinData.minerId3, normalVector * (magnitude + 1f), false);
        }
    }
}
