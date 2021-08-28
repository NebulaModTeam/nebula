using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class NewDroneOrderProcessor : PacketProcessor<NewDroneOrderPacket>
    {
        private IPlayerManager playerManager;

        public NewDroneOrderProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
        {
            // Host does not need to know about flying drones of other players if he is not on the same planet
            if (IsHost)
            {
                if (GameMain.mainPlayer.planetId != packet.PlanetId)
                    return;

                NebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    if (packet.Stage == 1 || packet.Stage == 2)
                    {
                        Multiplayer.Session.Drones.AddPlayerDronePlan(player.Id, packet.EntityId);
                    }
                    else if (packet.Stage == 3)
                    {
                        Multiplayer.Session.Drones.RemovePlayerDronePlan(player.Id, packet.EntityId);
                    }
                }
            }

            Multiplayer.Session.World.UpdateRemotePlayerDrone(packet);
        }
    }
}
