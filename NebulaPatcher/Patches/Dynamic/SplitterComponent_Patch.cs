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
        [HarmonyPatch(nameof(SplitterComponent.SetPriority))]
        public static void SetPriority_Postfix(SplitterComponent __instance, int slot, bool isPriority, int filter)
        {
            if (SimulatedWorld.Initialized && StorageManager.IsHumanInput)
            {
                LocalPlayer.Instance.SendPacketToLocalStar(new SplitterPriorityChangePacket(__instance.id, slot, isPriority, filter, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
