using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Warning;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlanetFactory))]
    internal class PlanetFactory_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlanetFactory.OnBeltBuilt))]
        private static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            /*
             * Calls
             * Multiplayer.Session.Factories.OnNewSetInserterPickTarget(objId, pickTarget, inserterId, offset, pointPos);
             * After
             * this.factorySystem.SetInserterPickTarget(inserterId, num6, num5 - num7);
            */
            CodeMatcher codeMatcher = new CodeMatcher(instructions, iLGenerator)
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

            List<CodeInstruction> setInserterTargetInsts = codeMatcher.InstructionsWithOffsets(-5, -1); // inserterId, pickTarget, offset
            CodeInstruction objIdInst = codeMatcher.InstructionAt(-13); // objId
            List<CodeInstruction> pointPosInsts = codeMatcher.InstructionsWithOffsets(8, 10); // pointPos

            codeMatcher = codeMatcher
                          .Advance(1)
                          .InsertAndAdvance(setInserterTargetInsts.ToArray())
                          .InsertAndAdvance(objIdInst)
                          .InsertAndAdvance(pointPosInsts.ToArray())
                          .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int, int, int, int, UnityEngine.Vector3>>((inserterId, pickTarget, offset, objId, pointPos) =>
                          {
                              if (Multiplayer.IsActive)
                              {
                                  Multiplayer.Session.Factories.OnNewSetInserterPickTarget(objId, pickTarget, inserterId, offset, pointPos);
                              }
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
                              if (Multiplayer.IsActive)
                              {
                                  Multiplayer.Session.Factories.OnNewSetInserterInsertTarget(objId, pickTarget, inserterId, offset, pointPos);
                              }
                          }));

            return codeMatcher.InstructionEnumeration();
        }

        public delegate bool BoundsChecker(PlanetFactory factory, int index);

        /*
         * it is very unlikely that this is needed, but it was reported by one user
         * somehow it can happen that a savegame breaks because the PlanetFactory.veinPool[].groupIndex points behind the bounds of PlanetFactory.planet.veinGroups[]
         * in that case we skip those entries and offer the host to remove those "bad" groupIndex entries (which may break other things but at least makes the save usable again)
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlanetFactory.Import))]
        public static IEnumerable<CodeInstruction> Import_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), "veinCursor")),
                    new CodeMatch(OpCodes.Blt))
                .CreateLabel(out Label jmpForLoopIncrement)
                .Start()
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_planet"),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "veinGroups")),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), "veinPool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), "groupIndex")),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup))
                .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldloc_S, 29))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<BoundsChecker>((PlanetFactory factory, int index) =>
                {
                    if (factory.veinPool[index].groupIndex >= factory.planet.veinGroups.Length && Multiplayer.IsActive)
                    {
                        if(Multiplayer.Session.LocalPlayer.IsClient)
                        {
                            WarningManager.DisplayTemporaryWarning("IndexOutOfBounds while importing factory data. Host should fix his savefile.", 15000);
                        }
                        else if(Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            List<int> faultyVeins = new List<int>();
                            for(int i = 1; i < factory.veinCursor; i++)
                            {
                                if(factory.veinPool[i].groupIndex >= factory.planet.veinGroups.Length)
                                {
                                    faultyVeins.Add(i);
                                }
                            }
                            InGamePopup.ShowError("IndexOutOfBounds", $"Nebula detected an IndexOutOfBounds error while importing PlanetFactory data. This is very weird and rare, but we can try to fix it. Keep in mind this will possibly end in weird results but may makes this save usable again. We would need to remove {faultyVeins.Count} veins from {factory.planet.displayName}. Make a backup before trying the fix!", "Ignore", "Try to fix it", new Action(delegate() { }), new Action(delegate ()
                            {
                                for(int i = 0; i < faultyVeins.Count; i++)
                                {
                                    factory.RemoveVeinWithComponents(faultyVeins[i]);
                                }
                                WarningManager.DisplayTemporaryWarning("Done performing the fix, hopefully you dont see me again.", 15000);
                            }));
                        }
                        return false;
                    }
                    return true;
                }))
                .Insert(new CodeInstruction(OpCodes.Brfalse, jmpForLoopIncrement));
            return matcher.InstructionEnumeration();
        }
    }
}
