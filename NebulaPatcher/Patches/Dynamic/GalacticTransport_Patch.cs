using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    internal class GalacticTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GalacticTransport.SetForNewGame))]
        public static void SetForNewGame_Postfix()
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                Multiplayer.Session.Network.SendPacket(new ILSRequestgStationPoolSync());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GalacticTransport.RemoveStationComponent))]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Ships.PatchLockILS;
        }
    }
}
