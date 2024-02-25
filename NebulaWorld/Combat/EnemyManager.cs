#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Combat.GroundEnemy;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Combat;

public class EnemyManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public readonly ToggleSwitch IsIncomingRelayRequest = new();

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
        if (!Multiplayer.Session.IsGameLoaded)
        {
            return;
        }
        // Place holder for future
        var _ = gameTick;
    }

    public void BroadcastBaseStatusPackets(EnemyDFGroundSystem enemySystem, long gameTick)
    {
        var factoryIndex = enemySystem.factory.index;
        var bases = enemySystem.bases;
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
            var levelChanged = packet.Level != dFbase.evolve.level;
            if (levelChanged || (hashId % 300) == (int)gameTick % 300)
            {
                // Update when base level changes, or every 5s
                packet.Record(in dFbase);
                var planetId = enemySystem.planet.id;
                if (levelChanged)
                    Multiplayer.Session.Server.SendPacket(packet);
                else
                    Multiplayer.Session.Server.SendPacketToPlanet(packet, planetId);
            }
        }
    }

    public static void SetPlanetFactoryNextEnemyId(PlanetFactory factory, int enemyId)
    {
        if (enemyId >= factory.enemyCursor)
        {
            factory.enemyCursor = enemyId;
            while (factory.enemyCursor >= factory.enemyCapacity)
            {
                factory.SetEnemyCapacity(factory.enemyCapacity * 2);
            }
        }
        else
        {
            factory.enemyRecycle[0] = enemyId;
            factory.enemyRecycleCursor = 1;
        }
    }

    public static void SetSpaceSectorNextEnemyId(int enemyId)
    {
        var spaceSector = GameMain.spaceSector;

        if (enemyId >= spaceSector.enemyCursor)
        {
            spaceSector.enemyCursor = enemyId;
            while (spaceSector.enemyCursor >= spaceSector.enemyCapacity)
            {
                spaceSector.SetEnemyCapacity(spaceSector.enemyCapacity * 2);
            }
        }
        else
        {
            spaceSector.enemyRecycle[0] = enemyId;
            spaceSector.enemyRecycleCursor = 1;
        }
    }

    public static void SetSpaceSectorRecycle(int enemyCusor, int[] enemyRecycle)
    {
        var spaceSector = GameMain.spaceSector;

        spaceSector.enemyCursor = enemyCusor;
        var capacity = spaceSector.enemyCapacity;
        while (capacity <= spaceSector.enemyCursor)
        {
            capacity *= 2;
        }
        if (capacity > spaceSector.enemyCapacity)
        {
            spaceSector.SetEnemyCapacity(capacity);
        }
        spaceSector.enemyRecycleCursor = enemyRecycle.Length;
        Array.Copy(enemyRecycle, spaceSector.enemyRecycle, enemyRecycle.Length);
    }
}
