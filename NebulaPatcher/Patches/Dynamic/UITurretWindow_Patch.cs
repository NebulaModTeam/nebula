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

        //Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(__instance.turretId,
        // turret.itemId, turret.itemCount, turret.itemInc, GameMain.localPlanet?.id ?? -1));
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

    //TODO: Work In Progress on SuperNova
    //[HarmonyPostfix]
    //[HarmonyPatch(nameof(UITurretWindow.SuperNovaBtn_onClick))]
    //public static void OnSetSuperNova_Postfix(UITurretWindow __instance, int obj)
    //{
    //    if (!Multiplayer.IsActive)
    //        return;

    //    bool superNovaOn = __instance.defenseSystem.turrets.buffer[__instance.turretId].inSupernova;

    //    Multiplayer.Session.Network.SendPacketToLocalStar(new TurretSuperNovaPacket(__instance.turretId, superNovaOn,
    //        GameMain.localPlanet?.id ?? -1));
    //}


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
