using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    // Transpiler to sync the belt item filter on the ILS/PLS output
    [HarmonyPatch(typeof(UIBeltBuildTip))]
    internal class UIBeltBuildTip_Transpiler
    {
        private delegate int SetSlot(StationComponent stationComponent, int outputSlotId, int selectedIndex);

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIBeltBuildTip.SetFilterToEntity))]
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
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputSlotId))),
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.selectedIndex))))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<SetSlot>((StationComponent stationComponent, int outputSlotId, int selectedIndex) =>
                {
                    if (!Multiplayer.IsActive)
                    {
                        return 0;
                    }

                    if (Multiplayer.Session.Ships.ItemSlotStationId == stationComponent.id &&
                        Multiplayer.Session.Ships.ItemSlotStationGId == stationComponent.gid &&
                        Multiplayer.Session.Ships.ItemSlotLastSlotId == outputSlotId &&
                        Multiplayer.Session.Ships.ItemSlotLastSelectedIndex == selectedIndex)
                    {
                        return 0;
                    }

                    Multiplayer.Session.Network.SendPacketToLocalStar(new ILSUpdateSlotData(stationComponent.planetId, stationComponent.id, stationComponent.gid, outputSlotId, selectedIndex));
                    Multiplayer.Session.Ships.ItemSlotStationId = stationComponent.id;
                    Multiplayer.Session.Ships.ItemSlotStationGId = stationComponent.gid;
                    Multiplayer.Session.Ships.ItemSlotLastSlotId = outputSlotId;
                    Multiplayer.Session.Ships.ItemSlotLastSelectedIndex = selectedIndex;
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            return instructions;
        }
    }
}
