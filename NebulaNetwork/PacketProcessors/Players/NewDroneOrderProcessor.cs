#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class NewDroneOrderProcessor : PacketProcessor<NewDroneOrderPacket>
{
    private readonly IPlayerManager playerManager;

    public NewDroneOrderProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
    {
        // Host does not need to know about flying drones of other players if he is not on the same planet
        if (IsHost)
        {
            if (GameMain.mainPlayer.planetId != packet.PlanetId)
            {
                return;
            }

            var player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                switch (packet.Stage)
                {
                    case 1 or 2:
                        DroneManager.AddPlayerDronePlan(player.Id, packet.EntityId);
                        break;
                    case 3:
                        DroneManager.RemovePlayerDronePlan(player.Id, packet.EntityId);
                        break;
                }
            }
        }

        Multiplayer.Session.World.UpdateRemotePlayerDrone(packet);
    }
}
