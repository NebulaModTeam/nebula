﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaWorld;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

// Collections of patches that need to make game run in nographics mode
// This part only get patch when Multiplayer.IsDedicated is true
internal class Dedicated_Server_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMain), nameof(GameMain.Begin))]
    public static void GameMainBegin_Postfix()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        Log.Info($">> RemoteAccessEnabled: {Config.Options.RemoteAccessEnabled}");
        Log.Info(">> RemoteAccessPassword: " +
                 (string.IsNullOrWhiteSpace(Config.Options.RemoteAccessPassword) ? "None" : "Protected"));
        Log.Info($">> AutoPauseEnabled: {Config.Options.AutoPauseEnabled}");
        if (Config.Options.AutoPauseEnabled)
        {
            GameMain.Pause();
        }

        if (GameMain.mainPlayer != null)
        {
            // Don't let the player of dedicated server to interact with enemies
            GameMain.mainPlayer.isAlive = false;
        }
    }

    // Stop game rendering
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.OnDraw))]
    [HarmonyPatch(typeof(GameData), nameof(GameData.OnPostDraw))]
    [HarmonyPatch(typeof(FactoryModel), nameof(FactoryModel.LateUpdate))]
    [HarmonyPatch(typeof(SectorModel), nameof(SectorModel.LateUpdate))]
    public static bool OnDraw_Prefix()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameMain), nameof(GameMain.LateUpdate))]
    public static bool OnLateUpdate()
    {
        // Because UIRoot._LateUpdate() doesn't run in headless mode, we need this to enable autosave
        UIRoot.instance.uiGame.autoSave._LateUpdate();
        return false;
    }

    // Destory gameObject so Update() won't execute
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanetAtmoBlur), nameof(PlanetAtmoBlur.Start))]
    public static bool PlanetAtmoBlur_Start(PlanetAtmoBlur __instance)
    {
        Object.Destroy(__instance.gameObject);
        return false;
    }

    // RenderTexture is not support, so disable all functions using the constructor
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DysonMapCamera), nameof(DysonMapCamera.CheckOrCreateRTex))]
    [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CaptureScreenShot), typeof(int),
        typeof(int))] // Save won't have a img preview when disable
    [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CaptureScreenShot), typeof(int), typeof(int), typeof(string))]
    [HarmonyPatch(typeof(MechaEditorCamera), nameof(MechaEditorCamera.CheckOrCreateRTex))]
    [HarmonyPatch(typeof(UIDysonOrbitPreview), nameof(UIDysonOrbitPreview.CheckOrCreateRTex))]
    [HarmonyPatch(typeof(UIMechaMaterialBall), nameof(UIMechaMaterialBall._OnCreate))]
    [HarmonyPatch(typeof(UIMechaSaveGroup), nameof(UIMechaSaveGroup._OnCreate))]
    [HarmonyPatch(typeof(UIMilkyWay), nameof(UIMilkyWay.CheckOrCreateRTex))]
    [HarmonyPatch(typeof(UIMinimap3DControl), nameof(UIMinimap3DControl._OnCreate))]
    [HarmonyPatch(typeof(UISplitterWindow), nameof(UISplitterWindow._OnCreate))]
    [HarmonyPatch(typeof(UIStarmap), nameof(UIMilkyWay.CheckOrCreateRTex))]
    [HarmonyPatch(typeof(TranslucentImageSource), nameof(TranslucentImageSource.CreateNewBlurredScreen))]
    private static bool RenderTexture_Prefix()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ComputeShader), nameof(ComputeShader.FindKernel))]
    [HarmonyPatch(typeof(ComputeShader), nameof(ComputeShader.GetKernelThreadGroupSizes))]
    [HarmonyPatch(typeof(ComputeShader), nameof(ComputeShader.Dispatch))]
    public static bool ComputeShader_Prefix()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ComputeBuffer), nameof(ComputeBuffer.SetData), typeof(Array))]
    [HarmonyPatch(typeof(ComputeBuffer), nameof(ComputeBuffer.GetData), typeof(Array), typeof(int), typeof(int),
        typeof(int))] //DysonSwarm.Export
    public static bool ComputeBuffer_Prefix()
    {
        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(DysonSwarm), nameof(DysonSwarm.GameTick))]
    private static IEnumerable<CodeInstruction> DysonSwarmGameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Remove first part of the functions about computeShader
        // (this.computeShader.Set..., this.Dispatch_UpdateVel, this.Dispatch_UpdatePos)
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var matcher = new CodeMatcher(codeInstructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(DysonSwarm), nameof(DysonSwarm.Dispatch_UpdatePos))));
            var num = matcher.Pos + 1;
            matcher.Start()
                .RemoveInstructions(num);
            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("DysonSwarmGameTick_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DysonSwarm), nameof(DysonSwarm.SetSailCapacity))]
    public static void DysonSwarmSetSailCapacity_Prefix(DysonSwarm __instance)
    {
        // Skip the part of computeShader in if (this.swarmBuffer != null)
        // (this.computeShader.SetBuffer(...), this.Dispatch_BlitBuffer())
        if (__instance.swarmBuffer == null)
        {
            return;
        }
        __instance.swarmBuffer.Release();
        __instance.swarmInfoBuffer.Release();
        __instance.swarmBuffer = null;
        __instance.swarmInfoBuffer = null;
    }


    // From user report
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Graphic), nameof(Graphic.DoMeshGeneration))]
    [HarmonyPatch(typeof(Graphic), nameof(Graphic.DoLegacyMeshGeneration))]
    public static bool DoMeshGeneration_Prefix()
    {
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlanetATField), nameof(PlanetATField.RecalculatePhysicsShape))]
    public static void RecalculatePhysicsShape_Postfix(PlanetATField __instance)
    {
        // vanilla use GPU to calculate the shape of shield that is not fully covered the whole planet
        // In this patch it will act as it has been fully covered to make the planet shield effective
        if (!__instance.isEmpty)
        {
            __instance.isSpherical = true;
        }
    }

    // Fixes a Object Reference UI error when loading the Headless Server.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UICommunicatorIndicator), nameof(UICommunicatorIndicator._OnLateUpdate))]
    public static bool UICommunicatorIndicatorOnLateUpdate_Prefix()
    {
        return false;
    }
}
