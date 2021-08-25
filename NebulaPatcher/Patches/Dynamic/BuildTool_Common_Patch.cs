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
            if (!Multiplayer.IsActive)
                return true;


            List<BuildPreview> previews = __instance.buildPreviews;
            if (__instance is BuildTool_BlueprintPaste)
            {
                BuildTool_BlueprintPaste bpInstance = __instance as BuildTool_BlueprintPaste;
                previews = bpInstance.bpPool.Take(bpInstance.bpCursor).ToList();
            }

            // Host will just broadcast event to other players
            if (LocalPlayer.IsMasterClient)
            {
                int planetId = Multiplayer.Session.Factories.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                LocalPlayer.SendPacketToStar(new CreatePrebuildsRequest(planetId, previews, Multiplayer.Session.Factories.PacketAuthor == FactoryManager.AUTHOR_NONE ? LocalPlayer.PlayerId : Multiplayer.Session.Factories.PacketAuthor, __instance.GetType().ToString()), GameMain.galaxy.PlanetById(planetId).star.id);
            }

            //If client builds, he need to first send request to the host and wait for reply
            if (!LocalPlayer.IsMasterClient && !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, previews, Multiplayer.Session.Factories.PacketAuthor == FactoryManager.AUTHOR_NONE ? LocalPlayer.PlayerId : Multiplayer.Session.Factories.PacketAuthor, __instance.GetType().ToString()));
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
            if (Multiplayer.Session.Factories.IsIncomingRequest)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
