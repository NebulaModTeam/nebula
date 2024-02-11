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
            var packet = new DFGActivateBasePacket(__instance.localPlanet.id, dFGBase.id, false);
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
    [HarmonyPatch(nameof(PlayerAction_Combat.ShieldBurst))]
    public static void ShieldBurst_Postfix(PlayerAction_Combat __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value)
        {
            return;
        }

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
        if (!__result || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value)
        {
            return;
        }

        var ammoItemId = __instance.mecha.ammoItemId;
        var packet = new MechaShootPacket(Multiplayer.Session.LocalPlayer.Id,
            (byte)ammoType, ammoItemId, target.astroId, target.id);

        if (GameMain.localPlanet != null)
        {
            // Make sure the receiver has loaded factory
            Multiplayer.Session.Network.SendPacketToLocalPlanet(packet);
        }
    }
}
