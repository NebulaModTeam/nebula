using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMechaStatProcessor : PacketProcessor<PlayerMechaStat>
    {
        public override void ProcessPacket(PlayerMechaStat packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
            if (player != null)
            {                
                PlanetFactory factroy = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId)?.factory;
                if (factroy == null)
                {
                    // If client is in space, find the nearest planet factory base on its position
                    // code modfied from Player.get_nearestFactory

                    VectorLF3 uPosition = player.Data.UPosition.ToVectorLF3();
                    if (player.Data.LocalStarId > 0)
                    {
                        // find the nearest planet in the star system
                        double minDistance = double.MaxValue;
                        PlanetData[] planets = GameMain.galaxy.StarById(player.Data.LocalStarId).planets;
                        for (int i = 0; i < planets.Length; i++)
                        {
                            PlanetData planetData = planets[i];
                            if (planetData.factory != null)
                            {
                                double distance = (planetData.uPosition - uPosition).magnitude;
                                distance -= planetData.realRadius;
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    factroy = planetData.factory;
                                }
                            }
                        }
                    }

                    if (factroy == null)
                    {
                        // find the nearest planet from all factories
                        double minDistance = double.MaxValue;
                        PlanetFactory[] factories = GameMain.data.factories;
                        for (int i = 0; i < factories.Length; i++)
                        {
                            if (factories[i] != null)
                            {
                                double distance = (factories[i].planet.uPosition - uPosition).magnitude;
                                distance -= factories[i].planet.realRadius;
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    factroy = factories[i];
                                }
                            }
                        }
                    }
                }

                if (packet.ItemCount >= 0)
                {
                    GameMain.mainPlayer.mecha.AddProductionStat(packet.ItemId, packet.ItemCount, factroy);
                }
                else
                {
                    GameMain.mainPlayer.mecha.AddConsumptionStat(packet.ItemId, -packet.ItemCount, factroy);
                }
            }
        }
    }
}
