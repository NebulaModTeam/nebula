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
        [HarmonyPatch(nameof(CargoTraffic.PickupBeltItems))]
        public static void PickupBeltItems_Prefix()
        {
            if (SimulatedWorld.Initialized)
            {
                BeltManager.BeltPickupStarted();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CargoTraffic.PickupBeltItems))]
        public static void PickupBeltItems_Postfix()
        {
            if (SimulatedWorld.Initialized && GameMain.data.localPlanet != null)
            {
                BeltManager.BeltPickupEnded();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.PutItemOnBelt))]
        public static void PutItemOnBelt_Prefix(int beltId, int itemId)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                LocalPlayer.Instance.SendPacketToLocalStar(new BeltUpdatePutItemOnPacket(beltId, itemId, GameMain.data.localPlanet.id));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.AlterBeltRenderer))]
        public static bool AlterBeltRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.Instance.TargetPlanet == FactoryManager.Instance.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.RemoveBeltRenderer))]
        public static bool RemoveBeltRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.Instance.TargetPlanet == FactoryManager.Instance.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.AlterPathRenderer))]
        public static bool AlterPathRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.Instance.TargetPlanet == FactoryManager.Instance.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.RemovePathRenderer))]
        public static bool RemovePathRenderer_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.Instance.TargetPlanet == FactoryManager.Instance.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoTraffic.RefreshPathUV))]
        public static bool RefreshPathUV_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.Instance.TargetPlanet == FactoryManager.Instance.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }
    }
}
