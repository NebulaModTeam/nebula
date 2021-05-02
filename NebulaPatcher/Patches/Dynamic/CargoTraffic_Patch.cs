using HarmonyLib;
using NebulaModel.Packets.Belt;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(CargoTraffic))]
    class CargoTraffic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PickupBeltItems")]
        public static void PickupBeltItems_Prefix()
        {
            if (SimulatedWorld.Initialized)
            {
                BeltManager.BeltPickupStarted();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("PickupBeltItems")]
        public static void PickupBeltItems_Postfix()
        {
            if (SimulatedWorld.Initialized && GameMain.data.localPlanet != null)
            {
                BeltManager.BeltPickupEnded();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("PutItemOnBelt")]
        public static void PutItemOnBelt_Prefix(int beltId, int itemId)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
              LocalPlayer.SendPacketToLocalStar(new BeltUpdatePutItemOnPacket(beltId, itemId, GameMain.data.localPlanet.factoryIndex));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("AlterBeltRenderer")]
        public static bool AlterBeltRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveBeltRenderer")]
        public static bool RemoveBeltRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AlterPathRenderer")]
        public static bool AlterPathRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemovePathRenderer")]
        public static bool RemovePathRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RefreshPathUV")]
        public static bool RefreshPathUV_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }
    }
}
