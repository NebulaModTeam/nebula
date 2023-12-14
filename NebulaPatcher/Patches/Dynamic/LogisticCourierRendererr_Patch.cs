#region

using System;
using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(LogisticCourierRenderer))]
internal class LogisticCourierRendererr_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LogisticCourierRenderer.Update))]
    public static bool Update_Prefix(LogisticCourierRenderer __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        __instance.courierCount = 0;
        if (__instance.transport == null)
        {
            return false;
        }
        for (var i = 1; i < __instance.transport.dispenserCursor; i++)
        {
            var dispenserComponent = __instance.transport.dispenserPool[i];
            if (dispenserComponent == null || dispenserComponent.id != i)
            {
                continue;
            }
            var num = __instance.courierCount + dispenserComponent.workCourierCount;
            if (num <= 0)
            {
                continue;
            }
            while (__instance.capacity < num)
            {
                __instance.Expand2x();
            }
            Array.Copy(dispenserComponent.workCourierDatas, 0, __instance.couriersArr, __instance.courierCount,
                dispenserComponent.workCourierCount);
            __instance.courierCount = num;
        }

        // Add remote couriers animation
        var courierCount = __instance.courierCount + Multiplayer.Session.Couriers.CourierCount;
        if (courierCount > 0)
        {
            while (__instance.capacity < courierCount)
            {
                __instance.Expand2x();
            }
            Array.Copy(Multiplayer.Session.Couriers.CourierDatas, 0, __instance.couriersArr, __instance.courierCount,
                Multiplayer.Session.Couriers.CourierCount);
            __instance.courierCount = courierCount;
        }

        __instance.couriersBuffer?.SetData(__instance.couriersArr, 0, 0, __instance.courierCount);
        return false;
    }
}
