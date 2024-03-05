#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld.Chat.ChatLinks;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static

#endregion

namespace NebulaWorld.Combat;

public class EnemyManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public readonly ToggleSwitch IsIncomingRelayRequest = new();

    public readonly Dictionary<int, int[]> GroundTargets = [];

    public const bool DISABLE_DFCommunicator = true;

    private readonly Dictionary<int, DFGUpdateBaseStatusPacket> basePackets = [];
    private readonly Dictionary<int, DFHiveUpdateStatusPacket> hivePackets = [];


    public void Dispose()
    {
        GroundTargets.Clear();
        basePackets.Clear();
        hivePackets.Clear();
        GC.SuppressFinalize(this);
    }

    public void BroadcastBaseStatusPackets(EnemyDFGroundSystem enemySystem, long gameTick)
    {
        var factoryIndex = enemySystem.factory.index;
        var bases = enemySystem.bases;
        for (var baseId = 1; baseId < bases.cursor; baseId++)
        {
            var dFbase = bases.buffer[baseId];
            if (dFbase?.id != baseId) continue;

            var hashId = (factoryIndex << 16) | baseId; //assume max base count on a planet < 2^16
            if (!basePackets.TryGetValue(hashId, out var packet))
            {
                packet = new DFGUpdateBaseStatusPacket(in dFbase);
                basePackets.Add(hashId, packet);
            }
            var levelChanged = packet.Level != dFbase.evolve.level;
            if (levelChanged || (hashId % 120) == (int)gameTick % 120)
            {
                // Update when base level changes, or every 2s to players on that planet
                packet.Record(in dFbase);
                var planetId = enemySystem.planet.id;
                if (levelChanged)
                    Multiplayer.Session.Server.SendPacket(packet);
                else
                    Multiplayer.Session.Server.SendPacketToPlanet(packet, planetId);
            }
        }
    }

    public void BroadcastHiveStatusPackets(EnemyDFHiveSystem hive, long gameTick)
    {
        var hashId = hive.hiveAstroId;
        if (!hivePackets.TryGetValue(hashId, out var packet))
        {
            packet = new DFHiveUpdateStatusPacket(in hive);
            hivePackets.Add(hashId, packet);
        }
        var levelChanged = packet.Level != hive.evolve.level;
        if (levelChanged || (hashId % 600) == (int)gameTick % 600)
        {
            // Update when base level changes, or every 10s to local system
            packet.Record(in hive);
            if (levelChanged)
                Multiplayer.Session.Server.SendPacket(packet);
            else
                Multiplayer.Session.Server.SendPacketToStar(packet, hive.starData.id);
        }
    }

    public void DisplayPlanetPingMessage(string text, int planetId, Vector3 pos)
    {
        var planet = GameMain.galaxy.PlanetById(planetId);
        if (planet == null) return;

        var message = text + " [" + NavigateChatLinkHandler.FormatNavigateToPlanetPos(planetId, pos, planet.displayName) + "]";
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
    }

    public void DisplayAstroMessage(string text, int astroId)
    {
        string displayMessage = null;

        if (GameMain.galaxy.PlanetById(astroId) != null)
        {
            displayMessage = GameMain.galaxy.PlanetById(astroId).displayName;
        }
        else if (GameMain.galaxy.StarById(astroId / 100) != null)
        {
            displayMessage = GameMain.galaxy.StarById(astroId / 100).displayName;
        }
        if (displayMessage == null) return;

        var message = text + " [" + NavigateChatLinkHandler.FormatNavigateToAstro(astroId, displayMessage) + "]";
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
    }

    public void OnFactoryLoadFinished(PlanetFactory factory)
    {
        var planetId = factory.planetId;

        // Set GroundTargets to current values
        var targets = new int[factory.enemyCapacity];
        GroundTargets[planetId] = targets;
        var unitCursor = factory.enemySystem.units.cursor;
        var unitBuffer = factory.enemySystem.units.buffer;
        for (var i = 1; i < unitCursor; i++)
        {
            var enemyId = unitBuffer[i].enemyId;
            targets[enemyId] = unitBuffer[i].hatred.max.target;
        }
    }

    public void OnLeavePlanet()
    {
        if (Multiplayer.Session.IsServer) return;

        // Reset threat on each base on the loaded factory so it doesn't show on the monitor        
        for (var factoryIdx = 0; factoryIdx < GameMain.data.factoryCount; factoryIdx++)
        {
            var factory = GameMain.data.factories[factoryIdx];
            if (factory == null) continue;

            var bases = factory.enemySystem.bases;
            for (var baseId = 1; baseId < bases.cursor; baseId++)
            {
                if (bases[baseId] != null && bases[baseId].id == baseId && !bases[baseId].hasAssaultingUnit)
                {
                    bases[baseId].evolve.threat = 0;
                }
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
