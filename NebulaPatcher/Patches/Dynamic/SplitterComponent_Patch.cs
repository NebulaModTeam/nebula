using HarmonyLib;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(SplitterComponent))]
    class SplitterComponent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SplitterComponent.SetPriority))]
        public static void SetPriority_Postfix(SplitterComponent __instance, int slot, bool isPriority, int filter)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.Storage.IsHumanInput)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new SplitterPriorityChangePacket(__instance.id, slot, isPriority, filter, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}
