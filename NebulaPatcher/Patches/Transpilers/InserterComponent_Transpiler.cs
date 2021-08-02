using BepInEx;
using HarmonyLib;
using NebulaWorld;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpilers
{
    delegate void debugPrint(ref InserterComponent _this, PlanetFactory factory, int entityId, int offset);

    [HarmonyPatch(typeof(InserterComponent))]
    public static class InserterComponent_Transpiler
    {
        public static ConcurrentDictionary<int, GameObject> FaultySortersText = new ConcurrentDictionary<int, GameObject>();
        static TextMesh uiSailIndicator_targetText = null;

        /*
         * this is part of the fix for 'Thread idx:2 Inserter Factory idx:0 inserter second gametick total cursor' index out of bounds PlanetFactory.PickFrom
         * basically some desync at some time produced faulty inserters that persist in the savegame and need to be replaced
         * we make sure players dont get spammed by errors and help them find them
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(InserterComponent.InternalUpdate))]
        public static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "pickTarget")),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "pickOffset")),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "filter")),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "PickFrom"),
                    new CodeMatch(OpCodes.Stloc_1))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "pickTarget")),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "pickOffset")),
                    HarmonyLib.Transpilers.EmitDelegate<debugPrint>((ref InserterComponent _this, PlanetFactory factory, int entityId, int offset) =>
                    {
                        if(!SimulatedWorld.Initialized || !LocalPlayer.IsMasterClient)
                        {
                            return;
                        }

                        int beltId = factory.entityPool[entityId].beltId;
                        if (beltId <= 0)
                        {
                            // this is the actual error trigger
                            if ((int)offset >= factory.powerSystem.genPool.Length)
                            {
                                if(!FaultySortersText.ContainsKey(entityId))
                                {
                                    // We must use ThreadingHelper in order to ensure this runs on the main thread, otherwise this will trigger a crash
                                    Vector3 pos = _this.pos2;
                                    Quaternion rot = _this.rot2;
                                    ThreadingHelper.Instance.StartSyncInvoke(() => addFaultyText(entityId, factory, pos, rot));
                                }
                            }
                        }

                    }));

            instructions = matcher
                .CreateLabelAt(matcher.Pos - 7, out var jmpLabel)
                .Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stloc_1),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InserterComponent), "careNeeds")),
                    new CodeMatch(OpCodes.Brfalse))
                .Set(OpCodes.Brfalse, jmpLabel)
                .InstructionEnumeration();

            return instructions;
        }

        public static void addFaultyText(int entityId, PlanetFactory factory, Vector3 pos, Quaternion rot)
        {
            GameObject textObject = new GameObject();

            if (FaultySortersText.TryAdd(entityId, textObject))
            {
                // Only get the field required if we actually need to, no point getting it every time
                if (uiSailIndicator_targetText == null)
                {
                    uiSailIndicator_targetText = (TextMesh)AccessTools.Field(typeof(UISailIndicator), "targetText").GetValue(UIRoot.instance.uiGame.sailIndicator);
                }

                // Make it follow the planets transform
                textObject.transform.SetParent(factory.planet.gameObject.transform, false);
                // Add a meshrenderer and textmesh component to show the text with a different font
                MeshRenderer meshRenderer = textObject.AddComponent<MeshRenderer>();
                TextMesh textMesh = textObject.AddComponent<TextMesh>();

                textObject.transform.position = pos;

                textMesh.text = "faulty sorter, replace me!";
                // Align it to be centered below them
                textMesh.anchor = TextAnchor.UpperCenter;

                // Copy the font over from the sail indicator
                textMesh.font = uiSailIndicator_targetText.font;
                meshRenderer.sharedMaterial = uiSailIndicator_targetText.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                textObject.SetActive(true);
            }
        }
    }
}
