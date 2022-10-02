using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDispenserWindow))]
    internal class UIDispenserWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDispenserWindow.OnCourierIconClick))]
        public static void OnCourierIconClick_Postfix(UIDispenserWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                DispenserComponent dispenserComponent = __instance.transport.dispenserPool[__instance.dispenserId];
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new DispenserSettingPacket(__instance.factory.planetId,
                                               __instance.dispenserId,
                                               EDispenserSettingEvent.SetCourierCount,
                                               dispenserComponent.workCourierCount + dispenserComponent.idleCourierCount));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDispenserWindow.OnCourierAutoReplenishButtonClick))]
        public static void OnCourierAutoReplenishButtonClick_Postfix(UIDispenserWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                DispenserComponent dispenserComponent = __instance.transport.dispenserPool[__instance.dispenserId];
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new DispenserSettingPacket(__instance.factory.planetId,
                                               __instance.dispenserId,
                                               EDispenserSettingEvent.ToggleAutoReplenish,
                                               dispenserComponent.courierAutoReplenish ? 1 : 0));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDispenserWindow.OnHoldupItemClick))]
        public static void OnHoldupItemClick_Postfix(UIDispenserWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                DispenserComponent dispenserComponent = __instance.transport.dispenserPool[__instance.dispenserId];
                if (__instance.player.inhandItemId == 0 && __instance.player.inhandItemCount == 0)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(
                        new DispenserStorePacket(__instance.factory.planetId,
                                                 in dispenserComponent));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDispenserWindow.OnMaxChargePowerSliderValueChange))]
        public static void OnMaxChargePowerSliderValueChange_Postfix(UIDispenserWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.StationsUI.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new DispenserSettingPacket(__instance.factory.planetId,
                                               __instance.dispenserId,
                                               EDispenserSettingEvent.SetMaxChargePower,
                                               BitConverter.ToInt32(BitConverter.GetBytes(value), 0)));
            }
        }
    }
}
