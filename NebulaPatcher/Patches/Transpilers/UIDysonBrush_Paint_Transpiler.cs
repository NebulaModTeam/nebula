#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIDysonBrush_Paint))]
internal class UIDysonBrush_Paint_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIDysonBrush_Paint._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static IEnumerable<CodeInstruction> _OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var matcher = new CodeMatcher(codeInstructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonNode), "color")));
            var pos1 = matcher.Pos;
            matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
            var pos2 = matcher.Pos;
            matcher.Advance(pos1 - pos2 + 1)
                .Insert(
                    new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                    HarmonyLib.Transpilers.EmitDelegate<Action<DysonNode>>(node =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return;
                        }
                        var starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
                        Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, node.layerId,
                            node.color, DysonSphereColorChangePacket.ComponentType.Node, node.id));
                    })
                );

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonFrame), "color")));
            pos1 = matcher.Pos;
            matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
            pos2 = matcher.Pos;
            matcher.Advance(pos1 - pos2 + 1)
                .Insert(
                    new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                    HarmonyLib.Transpilers.EmitDelegate<Action<DysonFrame>>(frame =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return;
                        }
                        var starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
                        Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, frame.layerId,
                            frame.color, DysonSphereColorChangePacket.ComponentType.Frame, frame.id));
                    })
                );

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonShell), "color")));
            pos1 = matcher.Pos;
            matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
            pos2 = matcher.Pos;
            matcher.Advance(pos1 - pos2 + 1)
                .Insert(
                    new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                    HarmonyLib.Transpilers.EmitDelegate<Action<DysonShell>>(shell =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return;
                        }
                        var starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
                        Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, shell.layerId,
                            shell.color, DysonSphereColorChangePacket.ComponentType.Shell, shell.id));
                    })
                );

            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("UIDysonBrush_Paint._OnUpdate_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }
}
