#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Turret;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITurretWindow))]
internal class UITurretWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnTurretShiftValueChange))]
    public static void OnTurretShiftValueChange_Postfix(UITurretWindow __instance)
    {
        //Notify about manual bullet inserting / withdrawing change
        if (!Multiplayer.IsActive || __instance is null)
        {
            return;
        }

        if (__instance._turretId == 0 || __instance.factory == null || __instance.player == null)
        {
            return;
        }
        ref var turret = ref __instance.defenseSystem.turrets.buffer[__instance.turretId];
        if (turret.id != __instance.turretId || turret.type != ETurretType.Disturb)
        {
            return;
        }

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretPhaseUpdatePacket(
            turret.id, turret.phasePos, __instance.factory.planetId));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnHandFillAmmoButtonClick))]
    public static void OnHandFillAmmoButtonClick_Postfix(UITurretWindow __instance)
    {
        //Notify about manual bullet inserting / withdrawing change
        if (!Multiplayer.IsActive || __instance is null)
        {
            return;
        }

        var defenseSystem = __instance.defenseSystem;

        // UITurretWindow closed
        if (defenseSystem is null)
        {
            return;
        }

        var turret = defenseSystem.turrets.buffer[__instance.turretId];

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(turret,
            GameMain.localPlanet?.id ?? -1));
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.AmmoBtn_onClick))]
    public static void AmmoButtonClick_Postfix(UITurretWindow __instance)
    {
        //Notify about manual bullet inserting / withdrawing change
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var turret = __instance.defenseSystem.turrets.buffer[__instance.turretId];

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(turret,
            GameMain.localPlanet?.id ?? -1));
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.ClearMagBtn_onClick))]
    public static void ClearMagClick_Postfix(UITurretWindow __instance)
    {
        //Notify about manual bullet inserting / withdrawing change
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var turret = __instance.defenseSystem.turrets.buffer[__instance.turretId];

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(turret,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.GroupSelectionBtn_onClick))]
    public static void OnSetGroup_Postfix(UITurretWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var group = __instance.defenseSystem.turrets.buffer[__instance.turretId].group;

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretGroupUpdatePacket(__instance.turretId, group,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.BurstModeBtn_onClick))]
    public static void OnSetBurstMode_Postfix(UITurretWindow __instance, int obj)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretBurstUpdatePacket(__instance.turretId, obj,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UITurretWindow.SuperNovaBtn_onClick))]
    public static bool OnSetSuperNova_Prefix(UITurretWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        if (__instance.turretId == 0 || __instance.factory == null || __instance.player == null)
        {
            return false;
        }
        ref var turret = ref __instance.defenseSystem.turrets.buffer[__instance.turretId];
        if (turret.id != __instance.turretId)
        {
            return false;
        }
        var packet = new TurretSuperNovaPacket(__instance.turretId,
            UITurretWindow.burstModeIndex, !turret.inSupernova, __instance.factory.planetId);

        if (Multiplayer.Session.IsClient)
        {
            // Client will wait for server to authorize
            Multiplayer.Session.Network.SendPacket(packet);
            return false;
        }
        else
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(packet);
            return true;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnPrioritySelectButtonClicked))]
    public static void OnSetPriority_Postfix(UITurretWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var vsSettings = __instance.defenseSystem.turrets.buffer[__instance.turretId].vsSettings;
        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretPriorityUpdatePacket(__instance.turretId, vsSettings,
            GameMain.localPlanet?.id ?? -1));
    }


    #region WIP / NOTES

    // VSMode clicked occurs on clicking one of the turret mode buttons

    // BurstMode updates index on window, and is used for supernova. Doesnt seem to have backend setting though

    #endregion
}
