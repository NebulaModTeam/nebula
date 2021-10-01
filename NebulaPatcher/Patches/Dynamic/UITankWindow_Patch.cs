using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UITankWindow))]
    internal class UITankWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnTakeBackPointerDown))]
        public static void OnTakeBackPointerDown_Postfix(UITankWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.IsTankWindowPointerPress = __instance.pointerPress;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnTakeBackPointerUp))]
        public static void OnTakeBackPointerUp_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.IsTankWindowPointerPress = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnApplicationFocus))]
        public static void OnApplicationFocus_Postfix(bool focus)
        {
            if (Multiplayer.IsActive)
            {
                if (!focus)
                {
                    Multiplayer.Session.IsTankWindowPointerPress = false;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnOutputSwitchClick))]
        public static void OnOutputSwitchClick_Postfix(UITankWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, false, __instance.storage.tankPool[__instance.tankId].outputSwitch, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow.OnInputSwitchClick))]
        public static void OnInputSwitchClick_Postfix(UITankWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, true, __instance.storage.tankPool[__instance.tankId].inputSwitch, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITankWindow._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnUpdate_Postfix(UITankWindow __instance)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.IsTankWindowPointerPress)
            {
                //Send update for inserting or withdrawing
                TankComponent thisTank = __instance.storage.tankPool[__instance.tankId];
                Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(__instance.tankId, thisTank.fluidId, thisTank.fluidCount, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
