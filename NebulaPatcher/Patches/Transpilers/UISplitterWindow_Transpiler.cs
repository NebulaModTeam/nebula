#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UISplitterWindow))]
internal class UISplitterWindow_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UISplitterWindow.OnCircleClick))]
    [HarmonyPatch(nameof(UISplitterWindow.OnCircleFilterRightClick))]
    [HarmonyPatch(nameof(UISplitterWindow.OnCircleRightClick))]
    private static IEnumerable<CodeInstruction> SetPriority_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Intercept SetPriority() with warper to broadcast the change
        try
        {
            var matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "SetPriority"))
                .Repeat(matcher => matcher
                    .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UISplitterWindow_Transpiler), nameof(SetPriority)))
                );
            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("UISpraycoaterWindow.SetPriority_Transpiler failed. Mod version not compatible with game version.");
            return instructions;
        }
    }

    private static void SetPriority(ref SplitterComponent splitter, int slot, bool isPriority, int filter)
    {
        splitter.SetPriority(slot, isPriority, filter);
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new SplitterPriorityChangePacket(splitter.id, slot, isPriority,
                filter, GameMain.localPlanet?.id ?? -1));
        }
    }
}
