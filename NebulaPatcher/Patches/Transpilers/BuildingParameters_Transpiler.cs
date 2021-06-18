using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(BuildingParameters))]
    public class BuildingParameters_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BuildingParameters.PasteToFactoryObject))]
        public static IEnumerable<CodeInstruction> PasteToFactoryObject_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            /*
             * Replaces
             *      factory.TakeBackItemsInEntity(mainPlayer, objectId);
             * With
             *      if(!SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient))
             *      {
             *         factory.TakeBackItemsInEntity(mainPlayer, objectId);
             *      }
            */
            var codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(false,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeBackItemsInEntity"));

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildingParameters.PasteToFactoryObject TakeBackItemsInEntity transpiler failed");
                return instructions;
            }

            instructions = codeMatcher
            .Repeat(matcher =>
            {
                matcher
                    .CreateLabelAt(matcher.Pos + 4, out Label label)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return !SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient);
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                    .Advance(5);
            })
            .InstructionEnumeration();

            /*
             * Replaces
             *      factorySystem.TakeBackItems_[Assembler||Lab](mainPlayer, [assemblerId||labId]);
             * With
             *      if(!SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient))
             *      {
             *         factorySystem.TakeBackItems_[Assembler||Lab](mainPlayer, [assemblerId||labId]);
             *      }
            */
            codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(false,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && (((MethodInfo)i.operand).Name == "TakeBackItems_Assembler" || ((MethodInfo)i.operand).Name == "TakeBackItems_Lab")));

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildingParameters.PasteToFactoryObject TakeBackItems_[Assembler||Lab] transpiler failed");
                return instructions;
            }

            return codeMatcher
            .Repeat(matcher =>
            {
                matcher
                    .CreateLabelAt(matcher.Pos + 4, out Label label)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return !SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient);
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                    .Advance(5);
            })
            .InstructionEnumeration();
        }
    }
}
