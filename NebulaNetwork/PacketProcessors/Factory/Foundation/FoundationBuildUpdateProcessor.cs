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
    private Vector3[] reformPoints = new Vector3[100];

    protected override void ProcessPacket(FoundationBuildUpdatePacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        var factory = IsHost ? GameMain.data.GetOrCreateFactory(planet) : planet?.factory;
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
            var reformPointsCount = factory.planet.aux.ReformSnap(packet.GroundTestPos.ToVector3(), packet.ReformSize,
                packet.ReformType, packet.ReformColor, reformPoints, packet.ReformIndices, factory.platformSystem,
                out var reformCenterPoint);
            factory.ComputeFlattenTerrainReform(reformPoints, reformCenterPoint, packet.Radius, reformPointsCount);
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                factory.FlattenTerrainReform(reformCenterPoint, packet.Radius, packet.ReformSize, packet.VeinBuried);
            }
            var area = packet.ReformSize * packet.ReformSize;
            for (var i = 0; i < area; i++)
            {
                var num71 = packet.ReformIndices[i];
                var platformSystem = factory.platformSystem;
                if (num71 < 0)
                {
                    continue;
                }
                var type = platformSystem.GetReformType(num71);
                var color = platformSystem.GetReformColor(num71);
                if (type == packet.ReformType && color == packet.ReformColor)
                {
                    continue;
                }
                factory.platformSystem.SetReformType(num71, packet.ReformType);
                factory.platformSystem.SetReformColor(num71, packet.ReformColor);
            }
        }

        if (!IsHost)
        {
            return;
        }
        if (planet != null)
        {
            Multiplayer.Session.Network.SendPacketToStar(packet, planet.star.id);
        }
    }
}
