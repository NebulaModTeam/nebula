#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

// Transpiler to sync the belt item filter on the ILS/PLS output
[HarmonyPatch(typeof(UIBeltBuildTip))]
internal class UIBeltBuildTip_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIBeltBuildTip.SetFilterToEntity))]
    private static IEnumerable<CodeInstruction> SetFilterToEntity_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*  Find:
         *      stationComponent.slots[this.outputSlotId].storageIdx = 1;
         *      stationComponent.slots[this.outputSlotId].storageIdx = this.selectedIndex;
         *  Insert SetSlot(StationComponent, int, int) afterward
         */
        var matcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Stfld))
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputSlotId))),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBeltBuildTip_Transpiler), nameof(SetSlot))))
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
                new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputSlotId))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.selectedIndex))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBeltBuildTip_Transpiler), nameof(SetSlot))));

        return matcher.InstructionEnumeration();
    }

    private static void SetSlot(StationComponent stationComponent, int outputSlotId, int selectedIndex)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        if (Multiplayer.Session.Ships.ItemSlotStationId == stationComponent.id &&
            Multiplayer.Session.Ships.ItemSlotStationGId == stationComponent.gid &&
            Multiplayer.Session.Ships.ItemSlotLastSlotId == outputSlotId &&
            Multiplayer.Session.Ships.ItemSlotLastSelectedIndex == selectedIndex)
        {
            return;
        }

        // Notify others about storageIdx changes
        Multiplayer.Session.Network.SendPacketToLocalStar(new ILSUpdateSlotData(stationComponent.planetId, stationComponent.id,
            stationComponent.gid, outputSlotId, selectedIndex));
        Multiplayer.Session.Ships.ItemSlotStationId = stationComponent.id;
        Multiplayer.Session.Ships.ItemSlotStationGId = stationComponent.gid;
        Multiplayer.Session.Ships.ItemSlotLastSlotId = outputSlotId;
        Multiplayer.Session.Ships.ItemSlotLastSelectedIndex = selectedIndex;
    }
}
