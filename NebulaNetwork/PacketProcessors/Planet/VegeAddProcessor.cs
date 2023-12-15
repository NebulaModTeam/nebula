#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

// Processes events for adding vegetation or veins in sandbox mode
[RegisterPacketProcessor]
internal class VegeAddProcessor : PacketProcessor<VegeAddPacket>
{
    protected override void ProcessPacket(VegeAddPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Planets.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            var factory = planet?.factory;
            if (factory == null)
            {
                return;
            }
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.EventFactory = factory;
            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            var pData = GameMain.gpuiManager.specifyPlanet;
            GameMain.gpuiManager.specifyPlanet = planet;

            if (packet.IsVein)
            {
                VeinData veinData = new();
                using (BinaryUtils.Reader reader = new(packet.Data))
                {
                    veinData.Import(reader.BinaryReader);
                }

                // Modify from PlayerAction_Plant.PlantVeinFinally
                factory.AssignGroupIndexForNewVein(ref veinData);
                var veinId = planet.factory.AddVeinData(veinData);
                var rot = Quaternion.FromToRotation(Vector3.up, veinData.pos.normalized);
                var veinPool = planet.factory.veinPool;
                if (planet == GameMain.localPlanet)
                {
                    // Only add model on local planet
                    veinPool[veinId].modelId =
                        planet.factoryModel.gpuiManager.AddModel(veinData.modelIndex, veinId, veinData.pos, rot);
                }

                var veinProto = LDB.veins.Select((int)veinData.type);
                var colliders = veinProto.prefabDesc.colliders;
                var num2 = 0;
                while (colliders != null && num2 < colliders.Length)
                {
                    veinPool[veinId].colliderId = planet.physics.AddColliderData(colliders[num2]
                        .BindToObject(veinId, veinPool[veinId].colliderId, EObjectType.Vein, veinData.pos, rot));
                    num2++;
                }
                factory.RefreshVeinMiningDisplay(veinId, 0, 0);
                factory.RecalculateVeinGroup(veinData.groupIndex);
            }
            else
            {
                VegeData vegeData = new();
                using (BinaryUtils.Reader reader = new(packet.Data))
                {
                    vegeData.Import(reader.BinaryReader);
                }

                // Modify from PlayerAction_Plant.PlantVegeFinally
                var num = planet.factory.AddVegeData(vegeData);
                var vegePool = planet.factory.vegePool;
                if (planet == GameMain.localPlanet)
                {
                    // Only add model on local planet
                    vegePool[num].modelId =
                        planet.factoryModel.gpuiManager.AddModel(vegeData.modelIndex, num, vegeData.pos, vegeData.rot);
                }

                var vegeProto = LDB.veges.Select(vegeData.protoId);
                var colliders = vegeProto.prefabDesc.colliders;
                var num2 = 0;
                while (colliders != null && num2 < colliders.Length)
                {
                    vegePool[num].colliderId = planet.physics.AddColliderData(colliders[num2].BindToObject(num,
                        vegePool[num].colliderId, EObjectType.Vegetable, vegeData.pos, vegeData.rot, Vector3.one));
                    num2++;
                }
                planet.physics.SetPlanetPhysicsColliderDirty();
            }

            GameMain.gpuiManager.specifyPlanet = pData;
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.EventFactory = null;
        }
    }
}
