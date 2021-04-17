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
            if (SimulatedWorld.Initialized)
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
    }
}
