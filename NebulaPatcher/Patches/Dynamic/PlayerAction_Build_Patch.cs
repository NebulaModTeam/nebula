using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreatePrebuilds")]
        public static bool CreatePrebuilds_Prefix(PlayerAction_Build __instance)
        {
            if (__instance.waitConfirm && VFInput._buildConfirm.onDown && __instance.buildPreviews.Count > 0)
            {
                if (!SimulatedWorld.Initialized)
                    return true;

                // Host will just broadcast event to other players
                if (LocalPlayer.IsMasterClient)
                {
                    int planetId = FactoryManager.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                    LocalPlayer.SendPacket(new CreatePrebuildsRequest(planetId, __instance.buildPreviews, __instance.previewPose));
                }

                //If client builds, he need to first send request to the host and wait for reply
                if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer)
                {
                    LocalPlayer.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, __instance.buildPreviews, __instance.previewPose));
                    return false;
                }
            }
            return true;
        }
    }
}
