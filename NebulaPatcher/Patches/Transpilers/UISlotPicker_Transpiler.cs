#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

// Transpiler to sync the belt item filter on the ILS/PLS input
[HarmonyPatch(typeof(UISlotPicker))]
public class UISlotPicker_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UISlotPicker.SetFilterToEntity))]
    public static IEnumerable<CodeInstruction> SetFilterToEntity_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        instructions = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Stfld))
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.outputSlotId))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.selectedIndex))))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<SetSlot>((stationComponent, outputSlotId, selectedIndex) =>
            {
                if (!Multiplayer.IsActive)
                {
                    return 0;
                }

                Multiplayer.Session.Network.SendPacketToLocalStar(new ILSUpdateSlotData(stationComponent.planetId,
                    stationComponent.id, stationComponent.gid, outputSlotId, selectedIndex));
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();
        return instructions;
    }

    private delegate int SetSlot(StationComponent stationComponent, int outputSlotId, int selectedIndex);
}
