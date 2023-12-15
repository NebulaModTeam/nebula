#region

using System.Collections.Generic;
using HarmonyLib;
using NebulaAPI.DataStructures;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DysonSphereLayer))]
internal class DysonSphereLayer_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.NewDysonNode))]
    public static void NewDysonNode_Prefix(DysonSphereLayer __instance, int protoId, Vector3 pos)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            return;
        }
        var nodeId = __instance.nodeRecycleCursor > 0
            ? __instance.nodeRecycle[__instance.nodeRecycleCursor - 1]
            : __instance.nodeCursor;
        Multiplayer.Session.Network.SendPacket(new DysonSphereAddNodePacket(__instance.starData.index, __instance.id,
            nodeId, protoId, new Float3(pos)));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.NewDysonFrame))]
    public static void NewDysonFrame_Prefix(DysonSphereLayer __instance, int protoId, int nodeAId, int nodeBId, bool euler)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            return;
        }
        var frameId = __instance.frameRecycleCursor > 0
            ? __instance.frameRecycle[__instance.frameRecycleCursor - 1]
            : __instance.frameCursor;
        Multiplayer.Session.Network.SendPacket(new DysonSphereAddFramePacket(__instance.starData.index, __instance.id,
            frameId, protoId, nodeAId, nodeBId, euler));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.RemoveDysonFrame))]
    public static void RemoveDysonFrame_Prefix(DysonSphereLayer __instance, int frameId)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereRemoveFramePacket(__instance.starData.index, __instance.id,
                frameId));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.RemoveDysonNode))]
    public static void RemoveDysonNode_Prefix(DysonSphereLayer __instance, int nodeId)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereRemoveNodePacket(__instance.starData.index, __instance.id,
                nodeId));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.NewDysonShell))]
    public static void NewDysonShell_Prefix(DysonSphereLayer __instance, int protoId, List<int> nodeIds)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            return;
        }
        var shellId = __instance.shellRecycleCursor > 0
            ? __instance.shellRecycle[__instance.shellRecycleCursor - 1]
            : __instance.shellCursor;
        Multiplayer.Session.Network.SendPacket(new DysonSphereAddShellPacket(__instance.starData.index, __instance.id,
            shellId, protoId, nodeIds));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.RemoveDysonShell))]
    public static void RemoveDysonShell_Prefix(DysonSphereLayer __instance, int shellId)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereRemoveShellPacket(__instance.starData.index, __instance.id,
                shellId));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonSphereLayer.InitOrbitRotation))]
    public static void InitOrbitRotation_Prefix(DysonSphereLayer __instance, Quaternion __1)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.DysonSpheres.IsIncomingRequest)
        {
            return;
        }
        //Send only when it's trigger by UIDELayerInfo.OnEditConfirmClick()
        if (UIRoot.instance.uiGame.dysonEditor.controlPanel.inspector.layerInfo.orbitEditMode)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereEditLayerPacket(__instance.starData.index, __instance.id,
                __1));
        }
    }
}
