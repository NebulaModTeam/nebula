using HarmonyLib;
using NebulaModel.Packets.Factory.Assembler;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIAssemblerWindow))]
    class UIAssemblerWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAssemblerWindow.OnRecipeResetClick))]
        public static void OnRecipeResetClick_Prefix(UIAssemblerWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.id, __instance.assemblerId, 0));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAssemblerWindow.OnRecipePickerReturn))]
        public static void OnRecipePickerReturn_Prefix(UIAssemblerWindow __instance, RecipeProto recipe)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerRecipeEventPacket(GameMain.data.localPlanet.id, __instance.assemblerId, recipe.ID));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAssemblerWindow.OnProductIcon0Click))]
        public static void OnProductIcon0Click_Prefix(UIAssemblerWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(0, __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[0], GameMain.data.localPlanet.id, __instance.assemblerId));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAssemblerWindow.OnProductIcon1Click))]
        public static void OnProductIcon1Click_Prefix(UIAssemblerWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(1, __instance.factorySystem.assemblerPool[__instance.assemblerId].produced[1], GameMain.data.localPlanet.id, __instance.assemblerId));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAssemblerWindow.OnManualServingContentChange))]
        public static void OnManualServingContentChange_Prefix(UIAssemblerWindow __instance)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            int[] update = new int[__instance.factorySystem.assemblerPool[__instance.assemblerId].served.Length];
            StorageComponent assemblerStorage = __instance.servingStorage;
            for (int i = 0; i < update.Length; i++)
            {
                update[i] = assemblerStorage.grids[i].count;
            }
            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateStoragePacket(update, GameMain.data.localPlanet.id, __instance.assemblerId));
        }
    }
}