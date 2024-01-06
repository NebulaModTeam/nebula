﻿#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Factory.Ejector;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIEjectorWindow))]
internal class UIEjectorWindow_Patch
{
    private static bool boost;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIEjectorWindow.OnManualServingContentChange))]
    public static void OnManualServingContentChange_Postfix(UIEjectorWindow __instance)
    {
        //Notify about manual bullet inserting / withdrawing change
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var storage = __instance.servingStorage;
        Multiplayer.Session.Network.SendToLocalStar(new EjectorStorageUpdatePacket(__instance.ejectorId,
            storage.grids[0].count, storage.grids[0].inc, GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIEjectorWindow.OnSetOrbit))]
    public static void OnSetOrbit_Postfix(UIEjectorWindow __instance, int orbitId)
    {
        //Notify about target orbit change
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendToLocalStar(new EjectorOrbitUpdatePacket(__instance.ejectorId, orbitId,
                GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIEjectorWindow.OnEjectorIdChange))]
    public static void OnEjectorIdChange_Postfix(UIEjectorWindow __instance)
    {
        if (Multiplayer.IsActive && __instance.active)
        {
            boost = __instance.boostSwitch.isOn;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIEjectorWindow._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Prefix(UIEjectorWindow __instance)
    {
        //Notify about boost change in sandbox mode
        if (!Multiplayer.IsActive || boost == __instance.boostSwitch.isOn)
        {
            return;
        }
        boost = __instance.boostSwitch.isOn;
        Multiplayer.Session.Network.SendToLocalStar(new EntityBoostSwitchPacket
            (GameMain.localPlanet?.id ?? -1, EBoostEntityType.Ejector, __instance.ejectorId, boost));
    }
}
