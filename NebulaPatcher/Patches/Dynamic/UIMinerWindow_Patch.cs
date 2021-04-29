using HarmonyLib;
using NebulaModel.Packets.Factory.Miner;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMinerWindow))]
    class UIMinerWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnProductIconClick")]
        public static void OnProductIconClick_Prefix(UIMinerWindow __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new MinerStoragePickupPacket(__instance.minerId, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}
