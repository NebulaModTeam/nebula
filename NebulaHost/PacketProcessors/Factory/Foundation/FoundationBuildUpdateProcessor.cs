using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using UnityEngine;

namespace NebulaHost.PacketProcessors.Factory.Foundation
{
    [RegisterPacketProcessor]
    class FoundationBuildUpdateProcessor : IPacketProcessor<FoundationBuildUpdatePacket>
    {
        public void ProcessPacket(FoundationBuildUpdatePacket packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);
            Vector3[] reformPoints = new Vector3[100];
            Vector3 reformCenterPoint = new Vector3();

            //Check if some mandatory variables are missing
            if (factory.platformSystem.reformData == null)
            {
                factory.platformSystem.InitReformData();
            }
            
            if (planet.physics == null || planet.physics.colChunks == null)
            {
                planet.physics = new PlanetPhysics(planet);
                planet.physics.Init();
            }
            if (planet.aux == null)
            {
                planet.aux = new PlanetAuxData(planet);
            }

            //Perform terrain operation
            int reformPointsCount = factory.planet.aux.ReformSnap(packet.GroundTestPos.ToVector3(), packet.ReformSize, packet.ReformType, packet.ReformColor, reformPoints, packet.ReformIndices, factory.platformSystem, out reformCenterPoint);
            factory.ComputeFlattenTerrainReform(reformPoints, reformCenterPoint, packet.Radius, reformPointsCount, 3f, 1f);
            using (FactoryManager.EventFromClient.On())
            {
                factory.FlattenTerrainReform(reformCenterPoint, packet.Radius, packet.ReformSize, packet.VeinBuried, 3f);
            }
            int area = packet.ReformSize * packet.ReformSize;
            for (int i = 0; i < area; i++)
            {
                int num71 = packet.ReformIndices[i];
                PlatformSystem platformSystem = factory.platformSystem;
                if (num71 >= 0)
                {
                    int type = platformSystem.GetReformType(num71);
                    int color = platformSystem.GetReformColor(num71);
                    if (type != packet.ReformType || color != packet.ReformColor)
                    {
                        factory.platformSystem.SetReformType(num71, packet.ReformType);
                        factory.platformSystem.SetReformColor(num71, packet.ReformColor);
                    }
                }
            }
        }
    }
}
