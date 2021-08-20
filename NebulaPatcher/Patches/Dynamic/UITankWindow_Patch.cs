using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UITankWindow))]
    class UITankWindow_Patch
    {
        public static bool pointerPress = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnTakeBackPointerDown))]
        public static void OnTakeBackPointerDown_Postfix(UITankWindow __instance)
        {
            pointerPress = __instance.pointerPress;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnTakeBackPointerUp))]
        public static void OnTakeBackPointerUp_Postfix(UITankWindow __instance)
        {
            pointerPress = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnApplicationFocus))]
        public static void OnApplicationFocus_Postfix(UITankWindow __instance, bool focus)
        {
            if (!focus)
            {
                pointerPress = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnOutputSwitchClick))]
        public static void OnOutputSwitchClick_Postfix(UITankWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, false, __instance.storage.tankPool[__instance.tankId].outputSwitch, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnInputSwitchClick))]
        public static void OnInputSwitchClick_Postfix(UITankWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, true, __instance.storage.tankPool[__instance.tankId].inputSwitch, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnUpdate_Postfix(UITankWindow __instance)
        {
            if (pointerPress && SimulatedWorld.Initialized)
            {
                //Send update for inserting or withdrawing
                TankComponent thisTank = __instance.storage.tankPool[__instance.tankId];
                LocalPlayer.SendPacketToLocalStar(new TankStorageUpdatePacket(__instance.tankId, thisTank.fluidId, thisTank.fluidCount, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
