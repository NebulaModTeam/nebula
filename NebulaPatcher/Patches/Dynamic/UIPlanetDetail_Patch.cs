#region

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIPlanetDetail))]
internal class UIPlanetDetail_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPlanetDetail.OnNameInputEndEdit))]
    public static void OnNameInputEndEdit_Postfix(UIPlanetDetail __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return;
        }
        if (__instance.planet != null && !string.IsNullOrEmpty(__instance.planet.overrideName))
        {
            // Send packet with new planet name
            Multiplayer.Session.Network.SendPacket(new NameInputPacket(__instance.planet.overrideName,
                NebulaModAPI.STAR_NONE, __instance.planet.id));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIPlanetDetail._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool _OnUpdate_Prefix(UIPlanetDetail __instance)
    {
        if (UIRoot.instance.galaxySelect == null || UIRoot.instance.galaxySelect.starmap == null ||
            Multiplayer.Session != null && !Multiplayer.Session.IsInLobby || Multiplayer.Session == null)
        {
            return true;
        }

        if (Time.frameCount % 30 == 0 && __instance.tabIndex == 0)
        {
            __instance.RefreshDynamicProperties();
            __instance.OnTabButtonClick(0); // baseInfoGroupGo.SetActive(true)
        }
        __instance.trslBg.SetActive(true);
        __instance.imgBg.SetActive(true);
        __instance.displayComboColorCard.color = ((__instance.uiGame.veinAmountDisplayFilter > 0) ? __instance.displayComboFilterColor : __instance.displayComboNormalColor);
        __instance.RefreshTabPanel();
        return false;
    }

    // temp set universe exploration to max for planet detail display in galaxy select screen
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIPlanetDetail.OnPlanetDataSet))]
    [HarmonyPatch(nameof(UIPlanetDetail.RefreshDynamicProperties))]
    public static void OnPlanetDataSet_Prefix()
    {
        if (Multiplayer.IsActive && Multiplayer.Session.IsInLobby)
        {
            GameMain.history.universeObserveLevel = 4;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPlanetDetail.OnPlanetDataSet))]
    [HarmonyPatch(nameof(UIPlanetDetail.RefreshDynamicProperties))]
    public static void OnPlanetDataSet_Postfix()
    {
        if (Multiplayer.IsActive && Multiplayer.Session.IsInLobby)
        {
            GameMain.history.universeObserveLevel = SimulatedWorld.GetUniverseObserveLevel();
        }
    }

    /// <summary>
    /// Show the memo button for planets that have memos in the global pool,
    /// even if the factory isn't loaded (unvisited planets).
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPlanetDetail.RefreshTabPanel))]
    public static void RefreshTabPanel_Postfix(UIPlanetDetail __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.planet == null) return;

        // If memoBtn is already active, nothing to do
        if (__instance.memoBtn.gameObject.activeSelf) return;

        // Check if there's a memo in the global todos pool for this planet
        var todos = GameMain.data?.galacticDigital?.todos;
        if (todos == null)
        {
            return;
        }

        var planetId = __instance.planet.id;
        for (int i = 1; i < todos.cursor; i++)
        {
            ref var todo = ref todos.buffer[i];
            if (todo.id == i && todo.ownerId == planetId &&
                todo.ownerType == ETodoModuleOwnerType.Astro)
            {
                // Found a memo for this planet - show the memo button
                __instance.memoBtn.gameObject.SetActive(true);
                break;
            }
        }
    }
}
