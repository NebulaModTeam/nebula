#region

using System;
using HarmonyLib;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(MechaArmorModel))]
internal class MechaArmorModel_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MechaArmorModel.SetDead))]
    public static bool SetDead_Prefix(MechaArmorModel __instance)
    {
        if (__instance.player.effect == null)
        {
            // Fix for remote players
            __instance._Close();
            __instance.gameObject.SetActive(false);
            return false;
        }
        return true;

    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MechaArmorModel.SetAlive))]
    public static bool SetAlive_Prefix(MechaArmorModel __instance)
    {
        if (__instance.player.effect == null)
        {
            // Fix for remote players
            __instance._Open();
            __instance.gameObject.SetActive(true);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MechaArmorModel.WreckagesRespawnLogic))]
    public static bool WreckagesRespawnLogic_Prefix(MechaArmorModel __instance, int tick)
    {
        if (MechaArmorModel.all_wreckages != null)
        {
            foreach (ArmorWreckage armorWreckage in MechaArmorModel.all_wreckages)
            {
                if (armorWreckage != null)
                {
                    // Expand armorWreckage.RespawnLogic(tick);
                    Transform transform;
                    if (armorWreckage.isPart)
                    {
                        transform = __instance.partModels[armorWreckage.armorId].transform;
                        armorWreckage.respawnMidPose.position = __instance.partModels[armorWreckage.armorId].part_renderer.bounds.center;
                    }
                    else
                    {
                        transform = __instance.boneModels[armorWreckage.armorId].meshObject.transform;
                        armorWreckage.respawnMidPose.position = transform.position + __instance.boneModels[armorWreckage.armorId].armorObject.transform.up * 0.5f;
                    }
                    armorWreckage.respawnStartPose.position = armorWreckage.trans.position;
                    armorWreckage.respawnStartPose.rotation = armorWreckage.trans.rotation;
                    armorWreckage.respawnMidPose.rotation = transform.rotation;
                    armorWreckage.respawnEndPose.position = transform.position;
                    armorWreckage.respawnEndPose.rotation = transform.rotation;
                    if (tick <= 60)
                    {
                        var num = Mathf.Clamp01(tick / 60f);
                        num = num * num * (3f - 2f * num);
                        armorWreckage.trans.position = Vector3.Lerp(armorWreckage.respawnStartPose.position, armorWreckage.respawnMidPose.position, num);
                        armorWreckage.trans.rotation = Quaternion.Slerp(armorWreckage.respawnStartPose.rotation, armorWreckage.respawnMidPose.rotation, num);
                        continue;
                    }
                    var num2 = Math.Min(60, armorWreckage.armorId / 5 * 10);
                    var num3 = Mathf.Clamp01((tick - 60 - num2) / 60f);
                    num3 = num3 * num3 * (3f - 2f * num3);
                    armorWreckage.trans.position = Vector3.Lerp(armorWreckage.respawnMidPose.position, armorWreckage.respawnEndPose.position, num3);
                    armorWreckage.trans.rotation = Quaternion.Slerp(armorWreckage.respawnMidPose.rotation, armorWreckage.respawnEndPose.rotation, num3);
                }
            }
        }
        return false;
    }
}
