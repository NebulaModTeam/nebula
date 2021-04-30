using HarmonyLib;
using NebulaModel.Packets.Factory.PowerExchanger;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPowerExchangerWindow))]
    class UIPowerExchangerWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnModeButtonClick")]
        public static void OnModeButtonClick_Prefix(UIPowerExchangerWindow __instance, int targetState)
        {
            //Notify other players about changing mode of the Power Exchenger
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new PowerExchangerChangeModePacket(__instance.exchangerId, targetState, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnEmptyOrFullUIButtonClick")]
        public static void OnEmptyOrFullUIButtonClick_Postfix(UIPowerExchangerWindow __instance, int itemId)
        {
            //Notify other about taking or inserting accumulators
            if (SimulatedWorld.Initialized)
            {
                PowerExchangerComponent powerExchangerComponent = __instance.powerSystem.excPool[__instance.exchangerId];
                LocalPlayer.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(__instance.exchangerId, powerExchangerComponent.emptyCount, powerExchangerComponent.fullCount, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}
