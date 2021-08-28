using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class PlanetFactory_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlanetFactory.OnBeltBuilt))]
        static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            /*
             * Calls
             * Multiplayer.Session.Factories.OnNewSetInserterPickTarget(objId, pickTarget, inserterId, offset, pointPos);
             * After
             * this.factorySystem.SetInserterPickTarget(inserterId, num6, num5 - num7);
            */
            var codeMatcher = new CodeMatcher(instructions, iLGenerator)
                                  .MatchForward(true,
                                    new CodeMatch(i => i.opcode == OpCodes.Callvirt &&
                                                       ((MethodInfo)i.operand).Name == nameof(FactorySystem.SetInserterPickTarget)
                                                 )
                                  );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlanetFactory_Transpiler.OnBeltBuilt 1 failed. Mod version not compatible with game version.");
                return instructions;
            }

            var setInserterTargetInsts = codeMatcher.InstructionsWithOffsets(-5, -1); // inserterId, pickTarget, offset
            var objIdInst = codeMatcher.InstructionAt(-13); // objId
            var pointPosInsts = codeMatcher.InstructionsWithOffsets(8, 10); // pointPos

            codeMatcher = codeMatcher
                          .Advance(1)
                          .InsertAndAdvance(setInserterTargetInsts.ToArray())
                          .InsertAndAdvance(objIdInst)
                          .InsertAndAdvance(pointPosInsts.ToArray())
                          .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int, int, int, int, UnityEngine.Vector3>>((inserterId, pickTarget, offset, objId, pointPos) =>
                          {
                              if (!Multiplayer.IsActive) return;
                              Multiplayer.Session.Factories.OnNewSetInserterPickTarget(objId, pickTarget, inserterId, offset, pointPos);
                          }));

            /*
             * Calls
             * Multiplayer.Session.Factories.OnNewSetInserterInsertTarget(objId, pickTarget, inserterId, offset, pointPos);
             * After
             * this.factorySystem.SetInserterInsertTarget(inserterId, num9, num5 - num10);
            */
            codeMatcher = codeMatcher
                          .MatchForward(true,
                          new CodeMatch(i => i.opcode == OpCodes.Callvirt &&
                                              ((MethodInfo)i.operand).Name == nameof(FactorySystem.SetInserterInsertTarget)
                                          )
                          );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlanetFactory_Transpiler.OnBeltBuilt 2 failed. Mod version not compatible with game version.");
                return codeMatcher.InstructionEnumeration();
            }

            setInserterTargetInsts = codeMatcher.InstructionsWithOffsets(-5, -1); // inserterId, pickTarget, offset
            objIdInst = codeMatcher.InstructionAt(-13); // objId
            pointPosInsts = codeMatcher.InstructionsWithOffsets(9, 11); // pointPos

            codeMatcher = codeMatcher
                          .Advance(1)
                          .InsertAndAdvance(setInserterTargetInsts.ToArray())
                          .InsertAndAdvance(objIdInst)
                          .InsertAndAdvance(pointPosInsts.ToArray())
                          .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int, int, int, int, UnityEngine.Vector3>>((inserterId, pickTarget, offset, objId, pointPos) =>
                          {
                              if (!Multiplayer.IsActive) return;
                              Multiplayer.Session.Factories.OnNewSetInserterInsertTarget(objId, pickTarget, inserterId, offset, pointPos);
                          }));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
