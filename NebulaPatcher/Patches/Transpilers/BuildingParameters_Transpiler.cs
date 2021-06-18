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
             * Wraps
             *      factory.TakeBackItemsInEntity()
             *      factorySystem.TakeBackItems_Assembler()
             *      factorySystem.TakeBackItems_Lab()
             * With
             *      if(!SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient))
             *      {
             *      }
            */
            var codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(false,
                    new CodeMatch(i => 
                        i.opcode == OpCodes.Callvirt && (
                            ((MethodInfo)i.operand).Name == nameof(PlanetFactory.TakeBackItemsInEntity) ||
                            ((MethodInfo)i.operand).Name == nameof(FactorySystem.TakeBackItems_Assembler) ||
                            ((MethodInfo)i.operand).Name == nameof(FactorySystem.TakeBackItems_Lab)
                        )
                    )
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildingParameters.PasteToFactoryObject transpiler failed");
                return instructions;
            }

            return codeMatcher
            .Repeat(matcher =>
            {
                matcher
                    .Advance(-3)
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
