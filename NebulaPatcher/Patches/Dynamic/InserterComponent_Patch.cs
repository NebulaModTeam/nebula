#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(InserterComponent))]
internal class InserterComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InserterComponent.InternalOffsetCorrection))]
    internal static bool InternalOffsetCorrection_Prefix(ref InserterComponent __instance, EntityData[] entityPool,
        CargoTraffic traffic, BeltComponent[] beltPool)
    {
        var flag = false;
        var beltId = entityPool[__instance.pickTarget].beltId;
        if (beltId > 0)
        {
            var cargoPath = traffic.GetCargoPath(beltPool[beltId].segPathId);
            if (cargoPath != null)
            {
                var num = beltPool[beltId].segPivotOffset + beltPool[beltId].segIndex;
                var num2 = num + __instance.pickOffset;
                if (num2 < 4)
                {
                    num2 = 4;
                }
                if (num2 + 5 >= cargoPath.pathLength)
                {
                    num2 = cargoPath.pathLength - 5 - 1;
                }
                if (__instance.pickOffset != (short)(num2 - num))
                {
                    Log.Warn(
                        $"{traffic.factory.planetId} Fix inserter{__instance.id} pickOffset {__instance.pickOffset} -> {num2 - num}");
                    __instance.pickOffset = (short)(num2 - num);
                    flag = true;
                }
            }
        }
        var beltId2 = entityPool[__instance.insertTarget].beltId;
        if (beltId2 > 0)
        {
            var cargoPath2 = traffic.GetCargoPath(beltPool[beltId2].segPathId);
            if (cargoPath2 != null)
            {
                var num3 = beltPool[beltId2].segPivotOffset + beltPool[beltId2].segIndex;
                var num4 = num3 + __instance.insertOffset;
                if (num4 < 4)
                {
                    num4 = 4;
                }
                if (num4 + 5 >= cargoPath2.pathLength)
                {
                    num4 = cargoPath2.pathLength - 5 - 1;
                }
                if (__instance.insertOffset != (short)(num4 - num3))
                {
                    Log.Warn(
                        $"{traffic.factory.planetId} Fix inserter{__instance.id} insertOffset {__instance.insertOffset} -> {num4 - num3}");
                    __instance.insertOffset = (short)(num4 - num3);
                    flag = true;
                }
            }
        }
        if (flag && Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new InserterOffsetCorrectionPacket(__instance.id,
                __instance.pickOffset, __instance.insertOffset, traffic.factory.planetId));
        }

        return false;
    }
}
