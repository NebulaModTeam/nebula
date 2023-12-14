#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerMechaStatProcessor : PacketProcessor<PlayerMechaStat>
{
    public override void ProcessPacket(PlayerMechaStat packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
        if (player != null)
        {
            var factory = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId)?.factory;
            if (factory == null)
            {
                // If client is in space, find the nearest planet factory base on its position
                // code modfied from Player.get_nearestFactory

                var uPosition = player.Data.UPosition.ToVectorLF3();
                if (player.Data.LocalStarId > 0)
                {
                    // find the nearest planet in the star system
                    var minDistance = double.MaxValue;
                    var planets = GameMain.galaxy.StarById(player.Data.LocalStarId).planets;
                    for (var i = 0; i < planets.Length; i++)
                    {
                        var planetData = planets[i];
                        if (planetData.factory != null)
                        {
                            var distance = (planetData.uPosition - uPosition).magnitude;
                            distance -= planetData.realRadius;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                factory = planetData.factory;
                            }
                        }
                    }
                }

                if (factory == null)
                {
                    // find the nearest planet from all factories
                    var minDistance = double.MaxValue;
                    var factories = GameMain.data.factories;
                    for (var i = 0; i < factories.Length; i++)
                    {
                        if (factories[i] != null)
                        {
                            var distance = (factories[i].planet.uPosition - uPosition).magnitude;
                            distance -= factories[i].planet.realRadius;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                factory = factories[i];
                            }
                        }
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
}
