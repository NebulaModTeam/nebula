using HarmonyLib;
using NebulaModel.Packets.Factory.Assembler;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIAssemblerWindow))]
    class UIAssemblerWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnRecipeResetClick")]
        public static void OnRecipeResetClick_Prefix(UIAssemblerWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.factoryIndex, __instance.assemblerId, 0));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnRecipePickerReturn")]
        public static void OnRecipePickerReturn_Prefix(UIAssemblerWindow __instance, RecipeProto recipe)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.factoryIndex, __instance.assemblerId, recipe.ID));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnProductIcon0Click")]
        public static void OnProductIcon0Click_Prefix(UIAssemblerWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(0, __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[0], GameMain.data.localPlanet.factoryIndex, __instance.assemblerId));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnProductIcon1Click")]
        public static void OnProductIcon1Click_Prefix(UIAssemblerWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(1, __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[1], GameMain.data.localPlanet.factoryIndex, __instance.assemblerId));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnManualServingContentChange")]
        public static void OnAssemblerIdChange_Prefix(UIAssemblerWindow __instance)
        {
            if (!SimulatedWorld.Initialized)
            {
                return;
            }

            int[] update = new int[__instance.factorySystem.assemblerPool[__instance.assemblerId].served.Length];
            StorageComponent assemblerStorage = (StorageComponent)AccessTools.Field(typeof(UIAssemblerWindow), "servingStorage").GetValue(__instance);
            for (int i = 0; i < update.Length; i++)
            {
                update[i] = assemblerStorage.grids[i].count;
            }
            LocalPlayer.SendPacketToLocalStar(new AssemblerUpdateStoragePacket(update, GameMain.data.localPlanet.factoryIndex, __instance.assemblerId));
        }
    }
}