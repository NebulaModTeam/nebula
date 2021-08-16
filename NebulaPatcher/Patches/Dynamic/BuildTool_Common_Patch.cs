using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Linq;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch]
    class BuildTool_Common_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
        public static bool CreatePrebuilds_Prefix(BuildTool __instance)
        {
            if (!SimulatedWorld.Initialized)
                return true;


            List<BuildPreview> previews = __instance.buildPreviews;
            if (__instance is BuildTool_BlueprintPaste)
            {
                BuildTool_BlueprintPaste bpInstance = __instance as BuildTool_BlueprintPaste;
                previews = bpInstance.bpPool.Take(bpInstance.bpCursor).ToList();
            }

            // Host will just broadcast event to other players
            if (LocalPlayer.Instance.IsMasterClient)
            {
                int planetId = FactoryManager.Instance.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                LocalPlayer.Instance.SendPacketToStar(new CreatePrebuildsRequest(planetId, previews, FactoryManager.Instance.PacketAuthor == FactoryManager.Instance.AUTHOR_NONE ? LocalPlayer.Instance.PlayerId : FactoryManager.Instance.PacketAuthor, __instance.GetType().ToString()), GameMain.galaxy.PlanetById(planetId).star.id);
            }

            //If client builds, he need to first send request to the host and wait for reply
            if (!LocalPlayer.Instance.IsMasterClient && !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                LocalPlayer.Instance.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, previews, FactoryManager.Instance.PacketAuthor == FactoryManager.Instance.AUTHOR_NONE ? LocalPlayer.Instance.PlayerId : FactoryManager.Instance.PacketAuthor, __instance.GetType().ToString()));
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CheckBuildConditions))]
        public static bool CheckBuildConditions(ref bool __result)
        {
            if (FactoryManager.Instance.IsIncomingRequest.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
