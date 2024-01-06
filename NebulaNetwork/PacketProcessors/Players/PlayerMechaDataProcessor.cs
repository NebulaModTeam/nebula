﻿#region

using NebulaAPI.Extensions;
using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class PlayerMechaDataProcessor : PacketProcessor<PlayerMechaData>
{
    public PlayerMechaDataProcessor()
    {
    }

    protected override void ProcessPacket(PlayerMechaData packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = Multiplayer.Session.Server.Players.GetByConnectionHandle(conn);

        //Find correct player for data to update, preserve sand count if syncing is enabled
        var sandCount = player.Data.Mecha.SandCount;
        player.Data.Mecha = packet.Data;
        if (Config.Options.SyncSoil)
        {
            player.Data.Mecha.SandCount = sandCount;
        }
    }
}
