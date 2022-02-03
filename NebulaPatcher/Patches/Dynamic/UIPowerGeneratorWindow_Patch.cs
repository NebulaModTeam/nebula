using HarmonyLib;
using NebulaModel.Packets.Factory.PowerGenerator;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPowerGeneratorWindow))]
    internal class UIPowerGeneratorWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnGammaMode1Click))]
        public static void OnGammaMode1Click_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about change of ray receiver to mode "electricity"
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Electricity, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnGammaMode2Click))]
        public static void OnGammaMode2Click_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about change of ray receiver to mode "produce photons"
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId, RayReceiverMode.Photon, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnCataButtonClick))]
        public static void OnCataButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of gravitational lens
            if (Multiplayer.IsActive)
            {
                RayReceiverChangeLensPacket packet = new RayReceiverChangeLensPacket(__instance.generatorId, 
                    __instance.powerSystem.genPool[__instance.generatorId].catalystPoint,
                    __instance.powerSystem.genPool[__instance.generatorId].catalystIncPoint,
                    GameMain.localPlanet?.id ?? -1);
                Multiplayer.Session.Network.SendPacketToLocalStar(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnFuelButtonClick))]
        public static void OnFuelButtonClick_Postfix(UIPowerGeneratorWindow __instance)
        {
            //Notify about changing amount of fuel in power plant
            if (Multiplayer.IsActive)
            {
                PowerGeneratorComponent thisComponent = __instance.powerSystem.genPool[__instance.generatorId];
                Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(__instance.generatorId, thisComponent.fuelId, thisComponent.fuelCount, thisComponent.fuelInc, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
