#region

using System;
using NebulaAPI;
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
internal class FoundationBuildUpdateProcessor : PacketProcessor<FoundationBuildUpdatePacket>
{
    private Vector3[] reformPoints = new Vector3[400];

    protected override void ProcessPacket(FoundationBuildUpdatePacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        var factory = planet?.factory;
        if (factory != null)
        {
            // Increase reformPoints for mods that increase brush size over 10
            if (packet.ReformSize * packet.ReformSize > reformPoints.Length)
            {
                reformPoints = new Vector3[packet.ReformSize * packet.ReformSize];
            }
            Array.Clear(reformPoints, 0, reformPoints.Length);

            //Check if some mandatory variables are missing
            if (factory.platformSystem.reformData == null)
            {
                factory.platformSystem.InitReformData();
            }

            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;

            //Perform terrain operation
            var center = packet.ExtraCenter.ToVector3();
            var area = packet.CirclePointCount;
            var costSandCount = 0; // dummy value, won't use
            var getSandCount = 0; // dummy value, won't use
            if (!packet.IsCircle) //Normal reform
            {
                var reformPointsCount = factory.planet.aux.ReformSnap(packet.GroundTestPos.ToVector3(), packet.ReformSize,
                    packet.ReformType, packet.ReformColor, reformPoints, packet.ReformIndices, factory.platformSystem,
                    out var reformCenterPoint);
                factory.ComputeFlattenTerrainReform(reformPoints, reformCenterPoint, packet.Radius, reformPointsCount, ref costSandCount, ref getSandCount);
                center = reformCenterPoint;
                area = packet.ReformSize * packet.ReformSize;
            }
            else //Remove pit
            {
                factory.ComputeFlattenTerrainReform(reformPoints, center, packet.Radius, packet.CirclePointCount, ref costSandCount, ref getSandCount, 3f, 1f);
            }
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                factory.FlattenTerrainReform(center, packet.Radius, packet.ReformSize, packet.VeinBuried);
            }
            var platformSystem = factory.platformSystem;
            for (var i = 0; i < area; i++)
            {
                var index = packet.ReformIndices[i];
                if (index < 0)
                {
                    continue;
                }
                var type = platformSystem.GetReformType(index);
                var color = platformSystem.GetReformColor(index);
                if (type != packet.ReformType || color != packet.ReformColor)
                {
                    factory.platformSystem.SetReformType(index, packet.ReformType);
                    factory.platformSystem.SetReformColor(index, packet.ReformColor);
                }
            }
        }

        if (IsHost && planet != null)
        {
            Multiplayer.Session.Network.SendPacketToStar(packet, planet.star.id);
        }
    }
}
