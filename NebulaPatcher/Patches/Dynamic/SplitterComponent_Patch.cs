using HarmonyLib;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(SplitterComponent))]
    class SplitterComponent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetPriority")]
        public static void SetPriority_Postfix(SplitterComponent __instance, int slot, bool isPriority, int filter)
        {
            if (SimulatedWorld.Initialized && StorageManager.IsHumanInput)
            {
                LocalPlayer.SendPacketToLocalStar(new SplitterPriorityChangePacket(__instance.id, slot, isPriority, filter, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}
