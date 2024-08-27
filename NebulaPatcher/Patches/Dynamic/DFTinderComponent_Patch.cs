#region

using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Combat.DFTinder;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DFTinderComponent))]
internal class DFTinderComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFTinderComponent.DispatchFromHive))]
    public static bool DispatchFromHive_Prefix(ref DFTinderComponent __instance, int _targetHiveAstroId)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        var hive1 = GameMain.spaceSector.GetHiveByAstroId(__instance.originHiveAstroId);
        var hive2 = GameMain.spaceSector.GetHiveByAstroId(_targetHiveAstroId);
        if (hive1 != null && hive2 != null)
        {
            __instance.targetHiveAstroId = _targetHiveAstroId;
            Multiplayer.Session.Network.SendPacket(new DFTinderDispatchPacket(__instance));
            var hasFactory = false;
            foreach (var planet in hive2.starData.planets)
            {
                if (planet != null && planet.factory != null && planet.factory.entityCount > 0)
                {
                    hasFactory = true;
                    break;
                }
            }
            if (hasFactory)
            {
                Multiplayer.Session.Enemies.SendAstroMessage("DF seed sent out".Translate(), hive1.starData.astroId, hive2.starData.astroId);
            }
        }
        return true;
    }
}
