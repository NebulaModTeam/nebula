#region

using System;
using System.Collections.Generic;
using NebulaModel.Packets.Combat.GroundEnemy;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Combat;

public class EnemyManager : IDisposable
{
    private readonly Dictionary<int, DFGUpdateBaseStatusPacket> basePackets = [];

    public EnemyManager()
    {
        basePackets.Clear();
    }

    public void Dispose()
    {
        basePackets.Clear();
        GC.SuppressFinalize(this);
    }

    public void GameTick(long gameTick)
    {
        if (!Multiplayer.Session.IsGameLoaded) return;
        if (Multiplayer.Session.IsClient) return;

        for (var factoryIndex = 0; factoryIndex < GameMain.data.factoryCount; factoryIndex++)
        {
            var bases = GameMain.data.factories[factoryIndex].enemySystem.bases;
            for (var baseId = 1; baseId < bases.cursor; baseId++)
            {
                var dFbase = bases.buffer[baseId];
                if (dFbase == null || dFbase.id != baseId) continue;

                var hashId = (factoryIndex << 16) | baseId; //assume max base count on a planet < 2^16
                if (!basePackets.TryGetValue(hashId, out var packet))
                {
                    packet = new DFGUpdateBaseStatusPacket(in dFbase);
                    basePackets.Add(hashId, packet);
                }
                if (packet.Level != dFbase.evolve.level || (hashId % 300) == (int)gameTick % 300)
                {
                    // Update when base level changes, or every 5s
                    packet.Record(in dFbase);
                    var planetId = GameMain.data.factories[factoryIndex].planet.id;
                    Multiplayer.Session.Network.SendPacketToPlanet(packet, planetId);
                }
            }
        }
    }
}
