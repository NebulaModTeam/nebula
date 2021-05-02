using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Logistics;
using System.Collections.Generic;
using System.Reflection.Emit;
using NebulaWorld.Logistics;

namespace NebulaPatcher.Patches.Transpilers
{
    // Transpiler to sync the belt item filter on the ILS/PLS output
    [HarmonyPatch(typeof(UIBeltBuildTip))]
    class UIBeltBuildTip_Transpiler
    {
        delegate int SetSlot(StationComponent stationComponent, int outputSlotId, int selectedIndex);

        [HarmonyTranspiler]
        [HarmonyPatch("SetFilterToEntity")]
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
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIBeltBuildTip), "outputSlotId")),
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIBeltBuildTip), "selectedIndex")))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<SetSlot>((StationComponent stationComponent, int outputSlotId, int selectedIndex) =>
                {
                    if (!SimulatedWorld.Initialized)
                    {
                        return 0;
                    }
                    if (ILSShipManager.ItemSlotStationId == stationComponent.id &&
                        ILSShipManager.ItemSlotStationGId == stationComponent.gid &&
                        ILSShipManager.ItemSlotLastSlotId == outputSlotId &&
                        ILSShipManager.ItemSlotLastSelectedIndex == selectedIndex)
                    {
                        return 0;
                    }

                    LocalPlayer.SendPacketToLocalStar(new ILSUpdateSlotData(stationComponent.planetId, stationComponent.id, stationComponent.gid, outputSlotId, selectedIndex));
                    ILSShipManager.ItemSlotStationId = stationComponent.id;
                    ILSShipManager.ItemSlotStationGId = stationComponent.gid;
                    ILSShipManager.ItemSlotLastSlotId = outputSlotId;
                    ILSShipManager.ItemSlotLastSelectedIndex = selectedIndex;
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            return instructions;
        }
    }
}
