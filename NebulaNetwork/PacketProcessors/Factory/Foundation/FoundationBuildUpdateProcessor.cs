using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using System;
using UnityEngine;
using FactoryManager = NebulaWorld.Factory.FactoryManager;

namespace NebulaNetwork.PacketProcessors.Factory.Foundation
{
    [RegisterPacketProcessor]
    class FoundationBuildUpdateProcessor : PacketProcessor<FoundationBuildUpdatePacket>
    {
        readonly Vector3[] reformPoints = new Vector3[100];

        public override void ProcessPacket(FoundationBuildUpdatePacket packet, NebulaConnection conn)
        {
            // TODO: MISSING CLIENT -> HOST -> CLIENT CODE 

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            PlanetFactory factory = IsHost ? GameMain.data.GetOrCreateFactory(planet) : planet?.factory;
            if (factory != null)
            {
                Array.Clear(reformPoints, 0, reformPoints.Length);

                //Check if some mandatory variables are missing
                if (factory.platformSystem.reformData == null)
                {
                    factory.platformSystem.InitReformData();
                }

                FactoryManager factoryManager = FactoryManager.Instance;

                factoryManager.TargetPlanet = packet.PlanetId;
                factoryManager.AddPlanetTimer(packet.PlanetId);
                factoryManager.TargetPlanet = NebulaModAPI.PLANET_NONE;

                //Perform terrain operation
                int reformPointsCount = factory.planet.aux.ReformSnap(packet.GroundTestPos.ToVector3(), packet.ReformSize, packet.ReformType, packet.ReformColor, reformPoints, packet.ReformIndices, factory.platformSystem, out Vector3 reformCenterPoint);
                factory.ComputeFlattenTerrainReform(reformPoints, reformCenterPoint, packet.Radius, reformPointsCount, 3f, 1f);
                using (factoryManager.IsIncomingRequest.On())
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
}
