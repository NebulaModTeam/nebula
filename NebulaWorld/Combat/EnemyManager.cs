#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Chat;
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

    public void SendPlanetPosMessage(string text, int planetId, Vector3 pos)
    {
        if (Multiplayer.Session.IsClient) return;
        var planet = GameMain.galaxy.PlanetById(planetId);
        if (planet == null) return;

        var message = text + " [" + NavigateChatLinkHandler.FormatNavigateToPlanetPos(planetId, pos, planet.displayName) + "]";
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
        Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.BattleMessage, message));
    }

    public void SendAstroMessage(string text, int astroId0, int astroId1 = 0)
    {
        if (Multiplayer.Session.IsClient) return;

        var message = text + " " + GetAstroLinkText(astroId0);
        if (astroId1 != 0) message += " => " + GetAstroLinkText(astroId1);
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
        Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.BattleMessage, message));
    }

    static string GetAstroLinkText(int astroId)
    {
        string displayName = null;
        if (GameMain.galaxy.PlanetById(astroId) != null)
        {
            displayName = GameMain.galaxy.PlanetById(astroId).displayName;
        }
        else if (GameMain.galaxy.StarById(astroId / 100) != null)
        {
            displayName = GameMain.galaxy.StarById(astroId / 100).displayName;
        }
        else if (GameMain.spaceSector.GetHiveByAstroId(astroId) != null)
        {
            displayName = GameMain.spaceSector.GetHiveByAstroId(astroId).displayName;
        }
        if (displayName == null) return "";
        return "[" + NavigateChatLinkHandler.FormatNavigateToAstro(astroId, displayName) + "]";
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
            // clear the blocking skill to prevent error due to skills are not all present in client
            unitBuffer[i].ClearBlockSkill();
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
            ref var ptr = ref factory.enemyPool[enemyId];
            if (ptr.id == enemyId)
            {
                // This is inside Combat.IsIncomingRequest, so it is approved and won't broadcast back to server
                Log.Warn("SetPlanetFactoryNextEnemyId: Kill ground enemy " + enemyId);
                ptr.isInvincible = false;
                factory.KillEnemyFinally(GameMain.mainPlayer, enemyId, ref CombatStat.empty);
            }

            factory.enemyRecycle[0] = enemyId;
            factory.enemyRecycleCursor = 1;
        }
    }

    public static void SetPlanetFactoryRecycle(PlanetFactory factory, int enemyCusor, int[] enemyRecycle)
    {
        // Make sure the enemyId about to use are empty
        for (var i = 0; i < enemyRecycle.Length; i++)
        {
            var enemyId = enemyRecycle[i];
            if (enemyId >= factory.enemyCursor) continue;

            ref var ptr = ref factory.enemyPool[enemyId];
            if (ptr.id == enemyId)
            {
                // This is inside Combat.IsIncomingRequest, so it is approved and won't broadcast back to server
                Log.Warn("SetPlanetFactoryRecycle: Kill ground enemy " + enemyId);
                ptr.isInvincible = false;
                factory.KillEnemyFinally(GameMain.mainPlayer, enemyId, ref CombatStat.empty);
            }
        }

        factory.enemyCursor = enemyCusor;
        var capacity = factory.enemyCapacity;
        while (capacity <= factory.enemyCursor)
        {
            capacity *= 2;
        }
        if (capacity > factory.enemyCapacity)
        {
            factory.SetEnemyCapacity(capacity);
        }
        factory.enemyRecycleCursor = enemyRecycle.Length;
        Array.Copy(enemyRecycle, factory.enemyRecycle, enemyRecycle.Length);
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
            ref var ptr = ref spaceSector.enemyPool[enemyId];
            if (ptr.id == enemyId)
            {
                // This is inside Enemies.IsIncomingRequest, so it is approved and won't broadcast back to server
                Log.Warn("SetSpaceSectorNextEnemyId: Kill space enemy " + enemyId);
                ptr.isInvincible = false;
                spaceSector.KillEnemyFinal(enemyId, ref CombatStat.empty);
            }
            spaceSector.enemyRecycle[0] = enemyId;
            spaceSector.enemyRecycleCursor = 1;
        }
    }

    public static void SetSpaceSectorRecycle(int enemyCusor, int[] enemyRecycle)
    {
        var spaceSector = GameMain.spaceSector;

        // Make sure the enemyId about to use are empty
        for (var i = 0; i < enemyRecycle.Length; i++)
        {
            var enemyId = enemyRecycle[i];
            if (enemyId >= spaceSector.enemyCursor) continue;

            ref var ptr = ref spaceSector.enemyPool[enemyId];
            if (ptr.id == enemyId)
            {
                // This is inside Enemies.IsIncomingRequest, so it is approved and won't broadcast back to server
                Log.Warn("SetSpaceSectorRecycle: Kill space enemy " + enemyId);
                ptr.isInvincible = false;
                spaceSector.KillEnemyFinal(enemyId, ref CombatStat.empty);
            }
        }

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
