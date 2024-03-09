#region

using HarmonyLib;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlayerAction_Combat))]
internal class PlayerAction_Combat_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ActivateBaseEnemyManually))]
    public static bool ActivateBaseEnemyManually_Prefix(PlayerAction_Combat __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
        {
            var raycastLogic = __instance.localPlanet.physics.raycastLogic;
            DFGBaseComponent dFGBase;
            if (raycastLogic.castEnemy.id > 0)
            {
                dFGBase = __instance.localPlanet.factory.enemySystem.bases.buffer[raycastLogic.castEnemy.owner];
            }
            else if (raycastLogic.castEnemyBaseId > 0)
            {
                dFGBase = __instance.localPlanet.factory.enemySystem.bases.buffer[raycastLogic.castEnemyBaseId];
            }
            else
            {
                return false;
            }

            if (dFGBase.activeTick > 0)
            {
                return false;
            }
            dFGBase.activeTick = 3; // keyTick = 1 sec, trigger every 3s
            var packet = new DFGActivateBasePacket(__instance.localPlanet.id, dFGBase.id, false);
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Server.SendPacketToLocalStar(packet);
                using (Multiplayer.Session.Combat.IsIncomingRequest.On())
                {
                    dFGBase.ActiveAllUnit(GameMain.gameTick);
                }
            }
            else
            {
                // Request for ActiveAllUnit approve
                Multiplayer.Session.Client.SendPacket(packet);
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ActivateNearbyEnemyBase))]
    public static bool ActivateNearbyEnemyBase_Prefix()
    {
        // Trigger nearby enemy base in DFGBaseComponent.UpdateHatred_Prefix
        return !Multiplayer.IsActive;
    }

    static long s_lastSentTime;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ActivateHiveEnemyManually))]
    public static bool ActivateHiveEnemyManually_Prefix(PlayerAction_Combat __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        var sendTimeDiff = GameMain.gameTick - s_lastSentTime;

        var spaceColliderLogic = __instance.spaceSector.physics.spaceColliderLogic;
        if (spaceColliderLogic.cursorCastAllCount > 0 && spaceColliderLogic.cursorCastAll[0].objType == EObjectType.Enemy)
        {
            ref var ptr = ref __instance.spaceSector.enemyPool[spaceColliderLogic.cursorCastAll[0].objId];
            __instance.spaceSector.TransformFromAstro_ref(ptr.astroId, out var centerUPos, ref ptr.pos);
            var hive = __instance.spaceSector.dfHivesByAstro[ptr.originAstroId - 1000000];
            if (hive.realized && (sendTimeDiff > 300 || sendTimeDiff < -300))
            {
                Multiplayer.Session.Client.SendPacket(new DFHiveUnderAttackRequest(hive.hiveAstroId, ref centerUPos, 5000f));
                s_lastSentTime = GameMain.gameTick;
            }
        }
        else if (spaceColliderLogic.castEnemyhiveIndex > 0)
        {
            var hive = __instance.spaceSector.dfHivesByAstro[spaceColliderLogic.castEnemyhiveIndex];
            if (hive.realized && (sendTimeDiff > 300 || sendTimeDiff < -300))
            {
                var packet = new DFHiveUnderAttackRequest
                {
                    HiveAstroId = hive.hiveAstroId
                };
                Multiplayer.Session.Client.SendPacket(packet);
                s_lastSentTime = GameMain.gameTick;
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ActivateNearbyEnemyHive))]
    public static bool ActivateNearbyEnemyHive(PlayerAction_Combat __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        // Check every 5s to wake up the nearby enemy hive
        if (__instance.localStar != null && GameMain.gameTick % 300 == 0)
        {
            for (var hive = __instance.spaceSector.dfHives[__instance.localStar.index]; hive != null; hive = hive.nextSibling)
            {
                if ((__instance.spaceSector.astros[hive.hiveAstroId - 1000000].uPos - __instance.player.uPosition).sqrMagnitude < 400000000.0)
                {
                    Multiplayer.Session.Client.SendPacket(new DFHiveUnderAttackRequest(hive.hiveAstroId, ref __instance.player.uPosition, 12000f));
                }
            }
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShieldBurst))]
    public static void ShieldBurst_Postfix(PlayerAction_Combat __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;

        var mecha = __instance.mecha;
        var packet = new MechaShieldBurstPacket(Multiplayer.Session.LocalPlayer.Id,
            mecha.energyShieldBurstProgress, mecha.energyShieldCapacity,
            mecha.energyShieldEnergy, mecha.energyShieldBurstDamageRate);

        if (GameMain.localStar != null)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(packet);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShootTarget))]
    public static void ShootTarget_Postfix(PlayerAction_Combat __instance, EAmmoType ammoType, in SkillTarget target, bool __result)
    {
        if (!__result || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;

        var ammoItemId = __instance.mecha.ammoItemId;
        var packet = new MechaShootPacket(Multiplayer.Session.LocalPlayer.Id,
            (byte)ammoType, ammoItemId, target.astroId, target.id);

        if (GameMain.localStar != null)
        {
            // Send to all players within the same system
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Server.SendPacketToLocalStar(packet);
            }
            else
            {
                Multiplayer.Session.Client.SendPacket(packet);
            }
        }
        else
        {
            // Outerspace: Broadcast to all players
            Multiplayer.Session.Network.SendPacket(packet);
        }
    }
}
