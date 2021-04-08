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
            if (StorageManager.IsHumanInput)
            {
                LocalPlayer.SendPacketToLocalPlanet(new SplitterPriorityChangePacket(__instance.id, slot, isPriority, filter));
            }
        }
    }
}
