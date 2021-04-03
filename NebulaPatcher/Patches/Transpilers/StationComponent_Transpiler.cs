using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NebulaWorld;
using NebulaModel.Packets.Logistics;

// thanks tanu and Therzok for the tipps!
namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(StationComponent))]
    class StationComponent_Transpiler
    {
        // desc of function to inject into InternalTickRemote after an AddItem() call
        delegate int ShipFunc(StationComponent stationComponent, ref ShipData shipData);

        [HarmonyTranspiler]
        [HarmonyPatch("InternalTickRemote")]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop)) // inject before this IL line
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) // load this ptr (pointing to the current StationComponent we are working on)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35)) // load the ShipData ptr of the current ship we are working on
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop)) // pop the loaded ShipData away
            .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ble),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130)) // the other StationComponent ('StationComponent stationComponent3 = gStationPool[shipData.otherGId];')
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Stfld)) // insert after 'shipData.itemCount = num77;' so we can query the itemCount out of the ShipData in the delegate
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(false, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();

            return instructions;
        }
    }
}
