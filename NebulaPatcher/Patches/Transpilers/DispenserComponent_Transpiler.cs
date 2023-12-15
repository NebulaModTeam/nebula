#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(DispenserComponent))]
public class DispenserComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DispenserComponent.InternalTick))]
    public static IEnumerable<CodeInstruction> InternalTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        // When courier go from idle to work, test if it is sent to player. If true, broadcast item decrease in distributor storage.
        // c# 87:
        //   int num11 = factory.PickFromStorage(num, itemId, num9, out inc);
        // >>  Replace with PickFromStorage(factory, num, itemId, num9, out inc) => DispenserAddTakePacket packet
        //   if (num11 > 0)
        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(i => i.IsLdloc()),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "PickFromStorage"))
            .RemoveInstruction()
            .Insert(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(DispenserComponent_Transpiler), "PickFromStorage")));

        //  When courier go from idle to work for player order, broadcast to all players on the same planet 
        //  c# 107: 
        //>>  Insert IdleCourierToWork(this) => IdleCourierToWork
        //  this.workCourierCount++;
        matcher
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "workCourierCount"),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "workCourierCount"))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DispenserComponent_Transpiler), "IdleCourierToWork")));

        //  c# 170: (same as above)
        matcher
            .Advance(8)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "workCourierCount"),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "workCourierCount"))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DispenserComponent_Transpiler), "IdleCourierToWork")));

        // When courier back to home, test if it recycles items from player. If true, broadcast item increase in distributor storage.
        // c# 604:
        //   bool useBan = this.orders[j].otherId >= 0;
        //   int num66;
        //   int num65 = factory.InsertIntoStorage(num, itemId6, itemCount2, this.workCourierDatas[j].inc, out num66, useBan);
        // >>  Replace with InsertIntoStorage(factory, entityId, itemId, count, inc, useBan) => DispenserAddTakePacket packet
        matcher.End()
            .MatchBack(true,
                new CodeMatch(OpCodes.Ldarg_1), // factory
                new CodeMatch(i => i.IsLdloc()), // entityId
                new CodeMatch(OpCodes.Ldloc_S), // itmeId
                new CodeMatch(OpCodes.Ldloc_S), // itemCount
                new CodeMatch(OpCodes.Ldarg_0), // this.workCourierDatas[j].inc,
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldloca_S), // remainInc
                new CodeMatch(OpCodes.Ldloc_S), // useBan
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "InsertIntoStorage"))
            .RemoveInstruction()
            .Insert(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(DispenserComponent_Transpiler), "InsertIntoStorage")));

        return matcher.InstructionEnumeration();
    }

    public static int PickFromStorage(PlanetFactory factory, int entityId, int itemId, int count, out int inc)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new DispenserAddTakePacket(factory.planetId,
                    entityId,
                    EDispenserAddTakeEvent.CourierTake,
                    itemId, count, 0));
        }
        return factory.PickFromStorage(entityId, itemId, count, out inc);
    }

    public static int InsertIntoStorage(PlanetFactory factory, int entityId, int itemId, int count, int inc, out int remainInc,
        bool useBan)
    {
        if (Multiplayer.IsActive && useBan == false)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new DispenserAddTakePacket(factory.planetId,
                    entityId,
                    EDispenserAddTakeEvent.CourierAdd,
                    itemId, count, inc));
        }
        return factory.InsertIntoStorage(entityId, itemId, count, inc, out remainInc, useBan);
    }

    public static void IdleCourierToWork(DispenserComponent dispenser)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalPlanet(
                new DispenserCourierPacket(GameMain.mainPlayer.planetId,
                    Multiplayer.Session.LocalPlayer.Id,
                    dispenser.id,
                    dispenser.workCourierDatas[dispenser.workCourierCount].itemId,
                    dispenser.workCourierDatas[dispenser.workCourierCount].itemCount));
        }
    }
}
