using HarmonyLib;
using NebulaModel;
using NebulaWorld;
using System;

namespace NebulaPatcher.Patches.Dynamic
{
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
            for (int i = 1; i < __instance.transport.dispenserCursor; i++)
            {
                DispenserComponent dispenserComponent = __instance.transport.dispenserPool[i];
                if (dispenserComponent != null && dispenserComponent.id == i)
                {
                    int num = __instance.courierCount + dispenserComponent.workCourierCount;
                    if (num > 0)
                    {
                        while (__instance.capacity < num)
                        {
                            __instance.Expand2x();
                        }
                        Array.Copy(dispenserComponent.workCourierDatas, 0, __instance.couriersArr, __instance.courierCount, dispenserComponent.workCourierCount);
                        __instance.courierCount = num;
                    }
                }
            }

            // Add remote couriers animation
            int courierCount = __instance.courierCount + Multiplayer.Session.Couriers.CourierCount;
            if (courierCount > 0)
            {
                while (__instance.capacity < courierCount)
                {
                    __instance.Expand2x();
                }
                Array.Copy(Multiplayer.Session.Couriers.CourierDatas, 0, __instance.couriersArr, __instance.courierCount, Multiplayer.Session.Couriers.CourierCount);
                __instance.courierCount = courierCount;
            }

            if (__instance.couriersBuffer != null)
            {
                __instance.couriersBuffer.SetData(__instance.couriersArr, 0, 0, __instance.courierCount);
            }
            return false;
        }
    }
}
