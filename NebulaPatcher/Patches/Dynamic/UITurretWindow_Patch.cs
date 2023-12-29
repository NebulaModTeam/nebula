#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Factory.Turret;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITurretWindow))]
internal class UITurretWindow_Patch
{

    //[HarmonyPostfix]
    //[HarmonyPatch(nameof(UITurretWindow.OnHandFillAmmoButtonClick))]
    //public static void OnManualServingContentChange_Postfix(UITurretWindow __instance)
    //{
    //    //Notify about manual bullet inserting / withdrawing change
    //    if (!Multiplayer.IsActive)
    //    {
    //        return;
    //    }

    //    var storage = __instance.servingStorage;
    //    Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(__instance.ejectorId,
    //        storage.grids[0].count, storage.grids[0].inc, GameMain.localPlanet?.id ?? -1));
    //}

    //[HarmonyPostfix]
    //[HarmonyPatch(nameof(UITurretWindow.OnHandFillAmmoButtonClick))]
    //public static void OnHandFillAmmoButtonClick_Postfix(UITurretWindow __instance)
    //{
    //    //Notify about manual bullet inserting / withdrawing change
    //    if (!Multiplayer.IsActive)
    //    {
    //        return;

    //    }

    //    var itemCount = __instance

    //    Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(__instance.turretId,
    //     --__instance.count, storage.grids[0].inc, GameMain.localPlanet?.id ?? -1));
    //}

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.GroupSelectionBtn_onClick))]
    public static void OnSetGroup_Postfix(UITurretWindow __instance, int value)
    {
        if (!Multiplayer.IsActive)
            return;

        byte group = __instance.defenseSystem.turrets.buffer[__instance.turretId].group;

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretGroupUpdatePacket(__instance.turretId, group,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.BurstModeBtn_onClick))]
    public static void OnSetBurstMode_Postfix(UITurretWindow __instance, int obj)
    {
        if (!Multiplayer.IsActive)
            return;

        int burstMode = UITurretWindow.burstModeIndex;

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretBurstUpdatePacket(__instance.turretId, burstMode,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.SuperNovaBtn_onClick))]
    public static void OnSetSuperNova_Postfix(UITurretWindow __instance, int obj)
    {
        if (!Multiplayer.IsActive)
            return;

        bool superNovaOn = __instance.defenseSystem.turrets.buffer[__instance.turretId].inSupernova;

        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretSuperNovaPacket(__instance.turretId, superNovaOn,
            GameMain.localPlanet?.id ?? -1));
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnPrioritySelectButtonClicked))]
    public static void OnSetPriority_Postfix(UITurretWindow __instance, int obj)
    {
        if (!Multiplayer.IsActive)
            return;

        var vsSettings = __instance.defenseSystem.turrets.buffer[__instance.turretId].vsSettings;
        Multiplayer.Session.Network.SendPacketToLocalStar(new TurretPriorityUpdatePacket(__instance.turretId, vsSettings,
            GameMain.localPlanet?.id ?? -1));
    }


    #region WIP / NOTES
    // VSMode clicked occurs on clicking one of the turret mode buttons

    // BurstMode updates index on window, and is used for supernove. Doesnt seem to have backend setting though


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnTurretIdChange))]
    public static void OnEjectorIdChange_Postfix(UITurretWindow __instance)
    {
        if (!Multiplayer.IsActive || !__instance.active)
        {
            return;
        }
    }

    #endregion

}
