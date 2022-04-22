using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(UIDysonBrush_Paint))]
    internal class UIDysonBrush_Paint_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIDysonBrush_Paint._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static IEnumerable<CodeInstruction> _OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try {
                int pos1, pos2;
                CodeMatcher matcher = new CodeMatcher(instructions);
                
                matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonNode), "color")));
                pos1 = matcher.Pos;
                matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
                pos2 = matcher.Pos;
                matcher.Advance(pos1 - pos2 + 1)
                    .Insert(
                        new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIDysonBrush_Paint_Transpiler), "SendNodePacket"))
                    );

                matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonFrame), "color")));
                pos1 = matcher.Pos;
                matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
                pos2 = matcher.Pos;
                matcher.Advance(pos1 - pos2 + 1)
                    .Insert(
                        new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIDysonBrush_Paint_Transpiler), "SendFramePacket"))
                    );

                matcher.MatchForward(false, new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonShell), "color")));
                pos1 = matcher.Pos;
                matcher.MatchBack(false, new CodeMatch(OpCodes.Ldloc_S));
                pos2 = matcher.Pos;
                matcher.Advance(pos1 - pos2 + 1)
                    .Insert(
                        new CodeInstruction(matcher.InstructionAt(pos2 - pos1 - 1)),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIDysonBrush_Paint_Transpiler), "SendShellPacket"))
                    );

                return matcher.InstructionEnumeration();
            }
            catch
            {
                NebulaModel.Logger.Log.Error("UIDysonBrush_Paint._OnUpdate_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
        }

        public static void SendNodePacket(DysonNode node)
        {
            int starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, node.layerId, node.color, DysonSphereColorChangePacket.ComponentType.Node, node.id));
        }

        public static void SendFramePacket(DysonFrame frame)
        {
            int starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, frame.layerId, frame.color, DysonSphereColorChangePacket.ComponentType.Frame, frame.id));
        }

        public static void SendShellPacket(DysonShell shell)
        {
            int starIndex = UIRoot.instance.uiGame.dysonEditor.selection.viewStar.index;
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, shell.layerId, shell.color, DysonSphereColorChangePacket.ComponentType.Shell, shell.id));
        }
    }
}
