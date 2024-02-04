#region

using HarmonyLib;
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
            var packet = new ActivateBasePacket(__instance.localPlanet.id, dFGBase.id, false);
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(packet);
                using (Multiplayer.Session.Combat.IsIncomingRequest.On())
                {
                    dFGBase.ActiveAllUnit(GameMain.gameTick);
                }
            }
            else
            {
                // Request for ActiveAllUnit
                Multiplayer.Session.Network.SendPacket(packet);
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ActivateNearbyEnemyBase))]
    public static bool ActivateNearbyEnemy_Prefix()
    {
        // Trigger nearby enemy in CombatManager.GameTick()
        return !Multiplayer.IsActive;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShootTarget))]
    public static void ShootTarget_Postfix(PlayerAction_Combat __instance, EAmmoType ammoType, in SkillTarget target, bool __result)
    {
        NebulaModel.Logger.Log.Debug($"{ammoType} {target.id} {__result}");
        if (!__result || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value)
        {
            return;
        }

        var isLocal = target.astroId == __instance.localAstroId;
        var ammoItemId = __instance.mecha.ammoItemId;
        var packet = new MechaShootPacket(Multiplayer.Session.LocalPlayer.Id,
            (byte)ammoType, ammoItemId, target.astroId, target.id);

        if (isLocal)
        {
            Multiplayer.Session.Network.SendPacketToLocalPlanet(packet);
        }
        else
        {
            // Currently mecha weapon can not cross star system, so broadcast only to local star
            Multiplayer.Session.Network.SendPacketToLocalStar(packet);
        }
    }
}
