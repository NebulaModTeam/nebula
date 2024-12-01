#if DEBUG

#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
// ReSharper disable RedundantAssignment

#endregion

namespace NebulaPatcher.Patches.Misc;

[HarmonyPatch(typeof(EnemyFormation))]
internal class Debug_EnemyFormation_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyFormation.AddUnit))]
    public static void AddUnit_Postfix(int __result)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer
            || Multiplayer.Session.Combat.IsIncomingRequest.Value
            || Multiplayer.Session.Enemies.IsIncomingRequest.Value) return;
        Log.Warn($"EnemyFormation.AddUnit {__result} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyFormation.RemoveUnit))]
    public static void RemoveUnit_Postfix(int port)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer
            || Multiplayer.Session.Combat.IsIncomingRequest.Value
            || Multiplayer.Session.Enemies.IsIncomingRequest.Value) return;
        Log.Warn($"EnemyFormation.RemoveUnit {port} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }
}

[HarmonyPatch(typeof(GameHistoryData))]
internal class Debug_GameHistoryData_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.dysonSphereSystemUnlocked), MethodType.Getter)]
    public static bool DysonSphereSystemUnlocked_Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.SetForNewGame))]
    public static void SetForNewGame_Postfix(GameHistoryData __instance)
    {
        __instance.dysonNodeLatitude = 90f;
    }
}

[HarmonyPatch(typeof(Mecha))]
internal class Debug_Mecha_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mecha.UseWarper))]
    public static void UseWarper_Postfix(ref bool __result)
    {
        __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mecha.SetForNewGame))]
    public static void SetForNewGame_Postfix(Mecha __instance)
    {
        if (GameMain.mainPlayer != __instance.player) return;
        __instance.coreEnergyCap = 30000000000;
        __instance.coreEnergy = 30000000000;
        __instance.corePowerGen = 5000000;
        __instance.reactorPowerGen = 20000000;
        __instance.coreLevel = 5;
        __instance.thrusterLevel = 5;
        __instance.maxSailSpeed = 2000f;
        __instance.maxWarpSpeed = 1000000f;
        __instance.walkSpeed = 25f;
        __instance.player.package.AddItemStacked(1803, 40, 1, out _); //add antimatter
        __instance.player.package.AddItemStacked(1501, 600, 1, out _); //add sails
        __instance.player.package.AddItemStacked(1503, 60, 1, out _); //add rockets
        __instance.player.package.AddItemStacked(2312, 10, 1, out _); //add launching silo
        __instance.player.package.AddItemStacked(2210, 10, 1, out _); //add artificial sun
        __instance.player.package.AddItemStacked(2311, 20, 1, out _); //add railgun
        __instance.player.package.AddItemStacked(2001, 600, 1, out _); //add MK3 belts
        __instance.player.package.AddItemStacked(2002, 600, 1, out _); //add MK3 belts
        __instance.player.package.AddItemStacked(2003, 600, 1, out _); //add MK3 belts
        __instance.player.package.AddItemStacked(2013, 100, 1, out _); //add MK3 inserters
        __instance.player.package.AddItemStacked(2212, 20, 1, out _); //add satellite sub-station
        __instance.player.package.AddItemStacked(1128, 100, 1, out _); // add combustible unit
        __instance.player.package.AddItemStacked(1601, 100, 1, out _); // add magnum ammo box
        __instance.player.package.AddItemStacked(1604, 100, 1, out _); // add shell set
        __instance.player.package.AddItemStacked(1607, 100, 1, out _); // add plasma capsule
        __instance.player.package.AddItemStacked(1609, 100, 1, out _); // add missile set
        __instance.player.package.AddItemStacked(1613, 100, 1, out _); // add jammer

        // temporary fix before PlayerTechBonuses update
        __instance.energyShieldUnlocked = true;
    }
}

[HarmonyPatch(typeof(MechaForge))]
internal class Debug_MechaForge_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MechaForge.TryAddTask))]
    public static void TryAddTask_Postfix(ref bool __result)
    {
        __result = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MechaForge.AddTaskIterate))]
    public static bool AddTaskIterate_Prefix(ref ForgeTask __result, int recipeId, int count)
    {
        var recipe = new ForgeTask(recipeId, count);
        foreach (var t in recipe.productIds)
        {
            GameMain.mainPlayer.package.AddItemStacked(t, count, 1, out _);
        }
        __result = null;
        return false;
    }
}

[HarmonyPatch(typeof(PlanetFactory))]
internal class Debug_PlanetFactory_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.AddEnemyDataWithComponents))]
    public static void AddEnemyDataWithComponents_Postfix(int __result)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;
        Log.Warn($"PlanetFactory.AddEnemyDataWithComponents {__result} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.RemoveEnemyWithComponents))]
    public static void RemoveEnemyWithComponents_Postfix(int id)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;
        Log.Warn($"PlanetFactory.RemoveEnemyWithComponents {id} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }
}

[HarmonyPatch(typeof(SpaceSector))]
internal class Debug_SpaceSector_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpaceSector.AddEnemyDataWithComponents))]
    public static void AddEnemyDataWithComponents_Postfix(int __result)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value || !Multiplayer.Session.IsGameLoaded) return;
        Log.Warn($"SpaceSector.AddEnemyDataWithComponents {__result} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpaceSector.RemoveEnemyWithComponents))]
    public static void RemoveEnemyWithComponents_Postfix(int id)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value) return;
        Log.Warn($"SpaceSector.RemoveEnemyWithComponents {id} without approve!");
        Log.Warn(System.Environment.StackTrace);
    }
}

[HarmonyPatch(typeof(UIAdvisorTip))]
internal class Debug_UIAdvisorTip_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIAdvisorTip.PlayAdvisorTip))]
    public static bool PlayAdvisorTip_Prefix()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIAdvisorTip.RunAdvisorTip))]
    public static bool RunAdvisorTip_Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(UITutorialTip))]
internal class Debug_UITutorialTip_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UITutorialTip.PopupTutorialTip))]
    public static bool PopupTutorialTip_Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(UIMechaEditor))]
internal class Debug_UIMechaEditor_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIMechaEditor.ApplyMechaAppearance))]
    public static bool ApplyMechaAppearance_Prefix(UIMechaEditor __instance)
    {
        __instance.mecha.diyAppearance.CopyTo(__instance.mecha.appearance);
        __instance.player.mechaArmorModel.RefreshAllPartObjects();
        __instance.player.mechaArmorModel.RefreshAllBoneObjects();
        __instance.mecha.appearance.NotifyAllEvents();
        __instance.CalcMechaProperty();

        return false;
    }
}

[HarmonyPatch(typeof(UIMechaMatsGroup))]
internal class Debug_UIMechaMatsGroup_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIMechaMatsGroup.TestMaterialEnough))]
    public static bool TestMaterialsEnough_Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}
#endif
