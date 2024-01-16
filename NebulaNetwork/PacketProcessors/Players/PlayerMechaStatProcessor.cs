#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerMechaStatProcessor : PacketProcessor<PlayerMechaStat>
{
    protected override void ProcessPacket(PlayerMechaStat packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = Players.Get(conn);
        if (player == null)
        {
            return;
        }
        var factory = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId)?.factory;
        if (factory == null)
        {
            // If client is in space, find the nearest planet factory base on its position
            // code modified from Player.get_nearestFactory

            var uPosition = player.Data.UPosition.ToVectorLF3();
            if (player.Data.LocalStarId > 0)
            {
                // find the nearest planet in the star system
                var minDistance = double.MaxValue;
                var planets = GameMain.galaxy.StarById(player.Data.LocalStarId).planets;
                foreach (var planetData in planets)
                {
                    if (planetData.factory == null)
                    {
                        continue;
                    }
                    var distance = (planetData.uPosition - uPosition).magnitude;
                    distance -= planetData.realRadius;
                    if (!(distance < minDistance))
                    {
                        continue;
                    }
                    minDistance = distance;
                    factory = planetData.factory;
                }
            }

            if (factory == null)
            {
                // find the nearest planet from all factories
                var minDistance = double.MaxValue;
                var factories = GameMain.data.factories;
                foreach (var t in factories)
                {
                    if (t == null)
                    {
                        continue;
                    }
                    var distance = (t.planet.uPosition - uPosition).magnitude;
                    distance -= t.planet.realRadius;
                    if (!(distance < minDistance))
                    {
                        continue;
                    }
                    minDistance = distance;
                    factory = t;
                }
            }
        }

        if (packet.ItemCount >= 0)
        {
            GameMain.mainPlayer.mecha.AddProductionStat(packet.ItemId, packet.ItemCount, factory);
        }
        else
        {
            GameMain.mainPlayer.mecha.AddConsumptionStat(packet.ItemId, -packet.ItemCount, factory);
        }
    }
}
