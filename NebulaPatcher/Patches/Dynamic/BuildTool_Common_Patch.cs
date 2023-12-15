#region

using System.Linq;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch]
internal class BuildTool_Common_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
    public static bool CreatePrebuilds_Prefix(BuildTool __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        var previews = __instance switch
        {
            BuildTool_BlueprintPaste bpInstance => bpInstance.bpPool.Take(bpInstance.bpCursor).ToList(),
            _ => __instance.buildPreviews
        };

        // Host will just broadcast event to other players
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            var planetId = Multiplayer.Session.Factories.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
            var authorId = Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
                ? Multiplayer.Session.LocalPlayer.Id
                : Multiplayer.Session.Factories.PacketAuthor;
            var prebuildId = Multiplayer.Session.Factories.GetNextPrebuildId(planetId);
            Multiplayer.Session.Network.SendPacketToStar(
                new CreatePrebuildsRequest(planetId, previews, authorId, __instance.GetType().ToString(), prebuildId),
                GameMain.galaxy.PlanetById(planetId).star.id);
        }

        //If client builds, he need to first send request to the host and wait for reply
        if (Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return true;
        }
        {
            if (!Multiplayer.Session.BuildTools.InitialCheck(previews[0].lpos))
            {
                return false;
            }
            var authorId = Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
                ? Multiplayer.Session.LocalPlayer.Id
                : Multiplayer.Session.Factories.PacketAuthor;
            Multiplayer.Session.Network.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, previews,
                authorId, __instance.GetType().ToString(), -1));
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CheckBuildConditions))]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CheckBuildConditions))]
    [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.CheckBuildConditions))]
    [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CheckBuildConditions))]
    [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CheckBuildConditions))]
    public static bool CheckBuildConditions(ref bool __result)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return true;
        }
        __result = true;
        return false;
    }
}
