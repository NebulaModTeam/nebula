using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class localPlanetSyncProcessor : IPacketProcessor<localPlanetSyncPckt>
    {
        private PlayerManager playerManager;

        public localPlanetSyncProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(localPlanetSyncPckt packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if(player != null)
            {
                packet.playerId = player.Id;
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
            else
            {
                player = playerManager.GetSyncingPlayer(conn);
                if(player != null)
                {
                    packet.playerId = player.Id;
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            if (packet.requestUpdate)
            {
                localPlanetSyncPckt Upacket = null;
                if (GameMain.localPlanet == null)
                {
                    Upacket = new localPlanetSyncPckt(0, false);
                }
                else
                {
                    Upacket = new localPlanetSyncPckt(GameMain.localPlanet.id, false);
                }
                Upacket.playerId = LocalPlayer.PlayerId;
                if(playerManager.SyncingPlayers.TryGetValue(conn, out Player p))
                {
                    Upacket.playerId = LocalPlayer.PlayerId;
                    p.SendPacket(Upacket);
                }
            }

            SimulatedWorld.UpdateRemotePlayerLocalPlanetId(packet);
        }
    }
}
