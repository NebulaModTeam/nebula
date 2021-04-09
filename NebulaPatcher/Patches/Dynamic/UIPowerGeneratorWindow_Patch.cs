using HarmonyLib;
using NebulaModel.Packets.Factory.PowerGenerator;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPowerGeneratorWindow))]
    class UIPowerGeneratorWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnGammaMode1Click")]
        public static void OnGammaMode1Click_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about change of ray receiver to mode "electricity"
            LocalPlayer.SendPacketToLocalPlanet(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Electricity));
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnGammaMode2Click")]
        public static void OnGammaMode2Click_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about change of ray receiver to mode "produce photons"
            LocalPlayer.SendPacketToLocalPlanet(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Photon));
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnCataButtonClick")]
        public static void OnCataButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of gravitational lens
            LocalPlayer.SendPacketToLocalPlanet(new RayReceiverChangeLensPacket(__instance.generatorId, __instance.powerSystem.genPool[__instance.generatorId].catalystPoint));
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnFuelButtonClick")]
        public static void OnFuelButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of fuel in power plant
            PowerGeneratorComponent thisComponent = __instance.powerSystem.genPool[__instance.generatorId];
            LocalPlayer.SendPacketToLocalPlanet(new PowerGeneratorFuelUpdatePacket(__instance.generatorId, thisComponent.fuelId, thisComponent.fuelCount));
        }

    }
}
