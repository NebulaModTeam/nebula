using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class StationUIProcessor : PacketProcessor<StationUI>
    {
        private readonly IPlayerManager playerManager;
        public StationUIProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // always update values for host, but he does not need to rely on the mimic flag (infact its bad for him)
                packet.ShouldMimic = false;
                Multiplayer.Session.StationsUI.UpdateUI(ref packet);

                // broadcast to every clients that may have the station loaded.
                playerManager.SendPacketToStarExcept(packet, packet.PlanetId, conn);

                // as we block the normal method for the client he must run it once he receives this packet.
                // but only the one issued the request should do it, we indicate this here
                packet.ShouldMimic = true;
                conn.SendPacket(packet);
            }

            if (IsClient)
            {
                Multiplayer.Session.StationsUI.UpdateUI(ref packet);
            }
        }
    }
}
