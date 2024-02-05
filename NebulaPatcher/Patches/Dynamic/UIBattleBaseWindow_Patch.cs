#region

using HarmonyLib;
using NebulaModel.Packets.Factory.BattleBase;
using NebulaWorld;
#pragma warning disable IDE1006

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIBattleBaseWindow))]
internal class UIBattleBaseWindow_Patch
{
    private static void SendEvent(UIBattleBaseWindow __instance, BattleBaseSettingEvent settingEvent, float arg)
    {
        // client will wait for server approve those interactions
        Multiplayer.Session.Network.SendPacket(new BattleBaseSettingUpdatePacket(
            __instance.factory.planetId, __instance.battleBaseId,
            settingEvent, arg));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnMaxChargePowerSliderChange))]
    public static bool OnMaxChargePowerSliderChange_Prefix(UIBattleBaseWindow __instance, float arg0)
    {
        if (!Multiplayer.IsActive || __instance.battleBaseId == 0 || __instance.factory == null ||
            __instance.battleBase.id != __instance.battleBaseId)
        {
            return true;
        }
        if (__instance.eventLock)
        {
            // Prevent ping-pong of sending update packets
            return false;
        }
        SendEvent(__instance, BattleBaseSettingEvent.ChangeMaxChargePower, arg0);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnDroneButtonClick))]
    public static bool OnDroneButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = __instance.constructionModule.droneEnabled ? 0f : 1f;
        SendEvent(__instance, BattleBaseSettingEvent.ToggleDroneEnabled, arg);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnDronePriorityButtonClick))]
    public static bool OnDronePriorityButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var dronePriority = __instance.constructionModule.dronePriority;
        var newDronePriority = (dronePriority + 1) % 3;
        SendEvent(__instance, BattleBaseSettingEvent.ChangeDronesPriority, newDronePriority);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnFleetButtonClick))]
    public static bool OnFleetButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = __instance.combatModule.moduleEnabled ? 0f : 1f;
        SendEvent(__instance, BattleBaseSettingEvent.ToggleCombatModuleEnabled, arg);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnAutoReconstructButtonClick))]
    public static bool OnAutoReconstructButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = __instance.constructionModule.autoReconstruct ? 0f : 1f;
        SendEvent(__instance, BattleBaseSettingEvent.ToggleAutoReconstruct, arg);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnAutoPickButtonClick))]
    public static bool OnAutoPickButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = __instance.battleBase.autoPickEnabled ? 0f : 1f;
        SendEvent(__instance, BattleBaseSettingEvent.ToggleAutoPickEnabled, arg);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnFleetTypeButtonClick))]
    public static bool OnFleetTypeButtonClick_Prefix(UIBattleBaseWindow __instance, int obj)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = ItemProto.kFighterGroundIds[obj];
        SendEvent(__instance, BattleBaseSettingEvent.ChangeFleetConfig, arg);
        return true; // Let local handles the storage/inventory interactions and broadcast the changes
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBattleBaseWindow.OnAutoReplenishButtonClick))]
    public static bool OnAutoReplenishButtonClick_Prefix(UIBattleBaseWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        var arg = __instance.combatModule.autoReplenishFleet ? 0f : 1f;
        SendEvent(__instance, BattleBaseSettingEvent.ToggleAutoReplenishFleet, arg);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }
}
