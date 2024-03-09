#region

using System;
using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class PlayerUpdateLocalStarIdProcessor : PacketProcessor<PlayerUpdateLocalStarId>
{
    protected override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var player = Players.Get(conn);
            if (player != null)
            {
                player.Data.LocalStarId = packet.StarId;
            }
            Server.SendPacketToStarExclude(packet, packet.StarId, conn);

            //Realize hives in this star system
            var star = GameMain.galaxy.StarById(packet.StarId);
            if (star != null)
            {
                var enemyDFHiveSystem = GameMain.spaceSector.dfHives[star.index];
                while (enemyDFHiveSystem != null)
                {
                    enemyDFHiveSystem.Realize();
                    enemyDFHiveSystem = enemyDFHiveSystem.nextSibling;
                }
            }
        }

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out var remotePlayerModel))
            {
                remotePlayerModel.Movement.LocalStarId = packet.StarId;
            }
        }
    }
}
