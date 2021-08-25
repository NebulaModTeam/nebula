using HarmonyLib;
using NebulaModel.Packets.Factory.PowerExchanger;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPowerExchangerWindow))]
    class UIPowerExchangerWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIPowerExchangerWindow.OnModeButtonClick))]
        public static void OnModeButtonClick_Prefix(UIPowerExchangerWindow __instance, int targetState)
        {
            //Notify other players about changing mode of the Power Exchenger
            if (Multiplayer.IsActive)
            {
                LocalPlayer.SendPacketToLocalStar(new PowerExchangerChangeModePacket(__instance.exchangerId, targetState, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPowerExchangerWindow.OnEmptyOrFullUIButtonClick))]
        public static void OnEmptyOrFullUIButtonClick_Postfix(UIPowerExchangerWindow __instance)
        {
            //Notify other about taking or inserting accumulators
            if (Multiplayer.IsActive)
            {
                PowerExchangerComponent powerExchangerComponent = __instance.powerSystem.excPool[__instance.exchangerId];
                LocalPlayer.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(__instance.exchangerId, powerExchangerComponent.emptyCount, powerExchangerComponent.fullCount, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
