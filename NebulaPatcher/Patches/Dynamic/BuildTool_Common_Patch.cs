using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch]
    class BuildTool_Common_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
        public static bool CreatePrebuilds_Prefix(BuildTool __instance)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // Host will just broadcast event to other players
            if (LocalPlayer.IsMasterClient)
            {
                int planetId = FactoryManager.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                LocalPlayer.SendPacketToStar(new CreatePrebuildsRequest(planetId, __instance.buildPreviews, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor, __instance.GetType().ToString()), GameMain.galaxy.PlanetById(planetId).star.id);
            }

            //If client builds, he need to first send request to the host and wait for reply
            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, __instance.buildPreviews, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor, __instance.GetType().ToString()));
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        public static void CreatePrebuilds_Postfix()
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient && FactoryManager.EventFromServer && FactoryManager.IsHumanInput)
            {
                FactoryManager.IsHumanInput = false;
            }
            else if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient && (FactoryManager.IsHumanInput || FactoryManager.IsFromClient))
            {
                FactoryManager.IsFromClient = false;
                FactoryManager.IsHumanInput = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CheckBuildConditions))]
        public static bool CheckBuildConditions(ref bool __result)
        {
            if (FactoryManager.EventFromClient)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
