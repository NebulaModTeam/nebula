#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Assembler;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIAssemblerWindow))]
internal class UIAssemblerWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnRecipeResetClick))]
    public static void OnRecipeResetClick_Postfix(UIAssemblerWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.id,
                __instance.assemblerId, 0));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnRecipePickerReturn))]
    public static void OnRecipePickerReturn_Postfix(UIAssemblerWindow __instance, RecipeProto recipe)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.id,
                __instance.assemblerId, recipe.ID));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnIncSwitchClick))]
    public static void OnIncSwitchClick_Postfix(UIAssemblerWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            // Notify others about production switch
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.id,
                __instance.assemblerId, -1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnProductIcon0Click))]
    public static void OnProductIcon0Click_Postfix(UIAssemblerWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(0,
                __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[0], GameMain.data.localPlanet.id,
                __instance.assemblerId));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnProductIcon1Click))]
    public static void OnProductIcon1Click_Postfix(UIAssemblerWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(1,
                __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[1], GameMain.data.localPlanet.id,
                __instance.assemblerId));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAssemblerWindow.OnManualServingContentChange))]
    public static void OnManualServingContentChange_Postfix(UIAssemblerWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var served = new int[__instance.factorySystem.assemblerPool[__instance.assemblerId].served.Length];
        var incServed = new int[served.Length];
        var assemblerStorage = __instance.servingStorage;
        for (var i = 0; i < served.Length; i++)
        {
            served[i] = assemblerStorage.grids[i].count;
            incServed[i] = assemblerStorage.grids[i].inc;
        }
        Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateStoragePacket(GameMain.data.localPlanet.id,
            __instance.assemblerId, served, incServed));
    }
}
