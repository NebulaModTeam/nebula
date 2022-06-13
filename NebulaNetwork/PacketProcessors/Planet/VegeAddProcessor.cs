using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.Planet
{    
    // Processes events for adding vegetation or veins in sandbox mode
    [RegisterPacketProcessor]
    internal class VegeAddProcessor : PacketProcessor<VegeAddPacket>
    {
        public override void ProcessPacket(VegeAddPacket packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Planets.IsIncomingRequest.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                if (planet?.factory == null)
                {
                    return;
                }
                Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
                Multiplayer.Session.Factories.EventFactory = planet.factory;
                Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
                PlanetData pData = GameMain.gpuiManager.specifyPlanet;
                GameMain.gpuiManager.specifyPlanet = planet;

                if (packet.IsVein)
                {
                    VeinData veinData = new();
                    using (BinaryUtils.Reader reader = new (packet.Data))
                    {
                        veinData.Import(reader.BinaryReader);
                    }

                    // Modify from PlayerAction_Plant.PlantVeinFinally
                    planet.AssignGroupForVein(ref veinData);
                    int veinId = planet.factory.AddVeinData(veinData);
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, veinData.pos.normalized);
                    VeinData[] veinPool = planet.factory.veinPool;
                    if (planet == GameMain.localPlanet)
                    {
                        // Only add model on local planet
                        veinPool[veinId].modelId = planet.factoryModel.gpuiManager.AddModel(veinData.modelIndex, veinId, veinData.pos, rot, true);
                    }

                    VeinProto veinProto = LDB.veins.Select((int)veinData.type);
                    ColliderData[] colliders = veinProto.prefabDesc.colliders;
                    int num2 = 0;
                    while (colliders != null && num2 < colliders.Length)
                    {
                        veinPool[veinId].colliderId = planet.physics.AddColliderData(colliders[num2].BindToObject(veinId, veinPool[veinId].colliderId, EObjectType.Vein, veinData.pos, rot));
                        num2++;
                    }
                    planet.factory.RefreshVeinMiningDisplay(veinId, 0, 0);

                    // PlayerAction_Plant.OnPlantVein(int veinId)
                    planet.RecalculateVeinGroupPos(planet.factory.veinPool[veinId].groupIndex);
                }
                else
                {
                    VegeData vegeData = new();
                    using (BinaryUtils.Reader reader = new(packet.Data))
                    {
                        vegeData.Import(reader.BinaryReader);
                    }

                    // Modify from PlayerAction_Plant.PlantVegeFinally
                    int num = planet.factory.AddVegeData(vegeData);
                    VegeData[] vegePool = planet.factory.vegePool;
                    if (planet == GameMain.localPlanet)
                    {
                        // Only add model on local planet
                        vegePool[num].modelId = planet.factoryModel.gpuiManager.AddModel(vegeData.modelIndex, num, vegeData.pos, vegeData.rot, true);
                    }

                    VegeProto vegeProto = LDB.veges.Select(vegeData.protoId);
                    ColliderData[] colliders = vegeProto.prefabDesc.colliders;
                    int num2 = 0;
                    while (colliders != null && num2 < colliders.Length)
                    {
                        vegePool[num].colliderId = planet.physics.AddColliderData(colliders[num2].BindToObject(num, vegePool[num].colliderId, EObjectType.Vegetable, vegeData.pos, vegeData.rot, Vector3.one));
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
}
