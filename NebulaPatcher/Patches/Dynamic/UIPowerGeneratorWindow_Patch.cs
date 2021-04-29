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
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Electricity, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnGammaMode2Click")]
        public static void OnGammaMode2Click_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about change of ray receiver to mode "produce photons"
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Photon, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnCataButtonClick")]
        public static void OnCataButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of gravitational lens
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new RayReceiverChangeLensPacket(__instance.generatorId, __instance.powerSystem.genPool[__instance.generatorId].catalystPoint, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnFuelButtonClick")]
        public static void OnFuelButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of fuel in power plant
            if (SimulatedWorld.Initialized)
            {
                PowerGeneratorComponent thisComponent = __instance.powerSystem.genPool[__instance.generatorId];
                LocalPlayer.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(__instance.generatorId, thisComponent.fuelId, thisComponent.fuelCount, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}
