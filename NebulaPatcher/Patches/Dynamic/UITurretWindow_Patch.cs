#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Factory;
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


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnPrioritySelectButtonClicked))]
    public static void OnSetPriority_Postfix(UITurretWindow __instance, int value)
    {
        //Notify about target orbit change
        if (Multiplayer.IsActive)
        {
            var vsSettings = __instance.defenseSystem.turrets.buffer[__instance.turretId].vsSettings;
            Multiplayer.Session.Network.SendPacketToLocalStar(new TurretPriorityUpdatePacket(__instance.turretId, vsSettings,
                GameMain.localPlanet?.id ?? -1));
        }
    }


    #region WIP/ NOTES
    // VSMode clicked occurs on clicking one of the turret mode buttons

    #endregion


    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITurretWindow.OnTurretIdChange))]
    public static void OnEjectorIdChange_Postfix(UITurretWindow __instance)
    {
        if (!Multiplayer.IsActive || !__instance.active)
        {
            return;
        }


        //if (Multiplayer.IsActive && __instance.active)
        //{
        //    boost = __instance.boostSwitch.isOn;
        //}
    }


}
