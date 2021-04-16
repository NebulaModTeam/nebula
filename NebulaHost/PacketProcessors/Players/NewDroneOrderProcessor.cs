using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Player;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class NewDroneOrderProcessor : IPacketProcessor<NewDroneOrderPacket>
    {
        private PlayerManager playerManager;

        public NewDroneOrderProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
        {
            //Host does not need to know about flying drones of other players if he is not on the same planet
            if (GameMain.mainPlayer.planetId != packet.PlanetId)
            {
                return;
            }

            Player player = playerManager.GetPlayer(conn);

            if (player != null)
            {
                if (packet.Stage == 1 || packet.Stage == 2)
                {
                    DroneManager.AddPlayerDronePlan(player.Id, packet.EntityId);
                }
                else if (packet.Stage == 3)
                {
                    DroneManager.RemovePlayerDronePlan(player.Id, packet.EntityId);
                }

                SimulatedWorld.UpdateRemotePlayerDrone(packet);
            }
        }
    }
}
