using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class NewDroneOrderProcessor : PacketProcessor<NewDroneOrderPacket>
    {
        private PlayerManager playerManager;

        public NewDroneOrderProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
        {
            // Host does not need to know about flying drones of other players if he is not on the same planet
            if (IsHost)
            {
                if (GameMain.mainPlayer.planetId != packet.PlanetId)
                    return;

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
                }
            }

            SimulatedWorld.UpdateRemotePlayerDrone(packet);
        }
    }
}
