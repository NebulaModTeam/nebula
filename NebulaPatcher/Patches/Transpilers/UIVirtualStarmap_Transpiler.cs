using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(UIVirtualStarmap))]
    class UIVirtualStarmap_Transpiler
    {
        private delegate void ShowSolarsystemDetails(UIVirtualStarmap starmap, int starIndex);
        private delegate bool IsBirthStar(UIVirtualStarmap starmap, int starIndex);
        private delegate bool IsBirthStar2(StarData starData, UIVirtualStarmap starmap);
        private delegate void TrackPlayerClick(UIVirtualStarmap starmap, int starIndex);

#pragma warning disable IDE1006
        public static bool pressSpamProtector = false;
        private static readonly float orbitScaler = 5f;

        public static int customBirthStar = -1;
        public static int customBirthPlanet = -1;
#pragma warning restore IDE1006
        /*
        if (flag2 && flag)
		{
			if (pressing)
			{
				this.starPool[j].nameText.text = this.starPool[j].textContent + "\r\n" + this.clickText.Translate();
			}
			this.starPool[j].nameText.rectTransform.sizeDelta = new Vector2(this.starPool[j].nameText.preferredWidth, this.starPool[j].nameText.preferredHeight);
		}

        to

        if (flag2 && pressing)
		{
			own logic
		}

        NOTE: the game does not use UIVirtualStarmap.clickText yet so the original logic would never be called anyways.

        Also change iteration over stars to start at 0 instead of 1 to also have a detailed solar system view of the default starting system
        By default the game always marks the first star as birth point, but as we can change that we also need to adapt the code for the visualisation
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIVirtualStarmap._OnLateUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static IEnumerable<CodeInstruction> _OnLateUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIVirtualStarmap), "starPool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_Item"),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIVirtualStarmap.StarNode), "nameText")),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_gameObject"),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "SetActive"),
                    new CodeMatch(OpCodes.Ldloc_S));
            if (matcher.IsInvalid)
            {
                Log.Warn("UIVirtualStarmap transpiler could not find injection point, not patching!");
                return instructions;
            }

            matcher.Advance(1)
                .SetAndAdvance(OpCodes.Ldloc_2, null) // change 'if (flag2 && flag)' to 'if (flag2 && pressing)'
                .Advance(2);

            // now remove original logic in this if(){}
            for(int i = 0; i < 39; i++)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            // add own logic
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, 12),
                HarmonyLib.Transpilers.EmitDelegate<ShowSolarsystemDetails>((UIVirtualStarmap starmap, int starIndex) =>
                {
                    if (pressSpamProtector)
                    {
                        return;
                    }
                    pressSpamProtector = true;

                    if (Multiplayer.Session != null && Multiplayer.Session.IsInLobby && starmap.clickText == "")
                    {
                        ClearStarmap(starmap);
                        ShowSolarSystem(starmap, starIndex);
                    }
                    else if(Multiplayer.Session != null && Multiplayer.Session.IsInLobby && starmap.clickText != "")
                    {
                        string[] split = starmap.clickText.Split(' ');
                        int starId = 0;

                        starId = Convert.ToInt32(split[0]);

                        StarData starData = starmap._galaxyData.StarById(starId); // no increment as we stored the actual id in there
                        if(starData == null || starIndex == 0) // starIndex == 0 is the star in the middle, so we need to decrement by 1 below
                        {
                            return;
                        }

                        PlanetData pData = starData.planets[starIndex - 1];
                        if(pData == null)
                        {
                            return;
                        }

                        if(UIRoot.instance.uiGame.planetDetail.planet != null && UIRoot.instance.uiGame.planetDetail.planet.id == pData.id && pData.type != EPlanetType.Gas)
                        {
                            // clicked on planet and details already visible, so set as new birth planet
                            starmap._galaxyData.birthStarId = starId;
                            starmap._galaxyData.birthPlanetId = pData.id;

                            GameMain.data.galaxy.birthStarId = starId;
                            GameMain.data.galaxy.birthPlanetId = pData.id;

                            customBirthStar = starData.id;
                            customBirthPlanet = pData.id;

                            Log.Info($"set birth planet{pData.id} {pData.displayName}");
                            Text text = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/start-button/start-text").GetComponent<Text>();
                            text.text = $"Start Game at {pData.displayName}";
                            text.horizontalOverflow = HorizontalWrapMode.Overflow;

                            if (pData.data == null)
                            {
                                // Part of PlanetModelingManager.PlanetCalculateThreadMain()
                                // Skip CalcWaterPercent() and CalculateVeinGroups() so those values won't reset
                                pData.data = new PlanetRawData(pData.precision);
                                pData.modData = pData.data.InitModData(pData.modData);
                                pData.data.CalcVerts();
                                pData.aux = new PlanetAuxData(pData);
                                PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(pData);
                                planetAlgorithm.GenerateTerrain(pData.mod_x, pData.mod_y);
                                pData.GenBirthPoints();
                            }
                        }

                        starmap.clickText = split[0] + " " + starIndex.ToString();
                        UIRoot.instance.uiGame.SetPlanetDetail(pData);

                        GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/right-group")?.SetActive(false);

                        UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(true);
                        UIRoot.instance.uiGame.planetDetail.gameObject.GetComponent<RectTransform>().parent.gameObject.SetActive(true);
                        UIRoot.instance.uiGame.planetDetail.gameObject.GetComponent<RectTransform>().parent.gameObject.GetComponent<RectTransform>().parent.gameObject.SetActive(true);

                        UIRoot.instance.uiGame.planetDetail._OnUpdate();
                    }
                }));

            // change for loop to start at 0 instead of 1
            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Stloc_2),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIVirtualStarmap), "clickText")),
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "IsNullOrEmpty"),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ceq),
                new CodeMatch(OpCodes.Stloc_3)
            )
            .Advance(1)
            .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0));

            // mark the correct star as birth point
            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Stloc_1),
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stloc_1),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Brtrue)
            )
            .Advance(-1)
            .SetAndAdvance(OpCodes.Nop, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
            .Insert(HarmonyLib.Transpilers.EmitDelegate<IsBirthStar>((UIVirtualStarmap starmap, int starIndex) =>
            {
                return starmap.starPool[starIndex].starData.id != starmap._galaxyData.birthStarId && starmap.starPool[starIndex].starData.id != starmap._galaxyData.birthPlanetId;
            }));

            // listen for general mouse clicks to deselect planet / solar system
            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIVirtualStarmap), "starPool")),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_Item"),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIVirtualStarmap.StarNode), "active")),
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ceq)
            )
            .Advance(3)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TrackPlayerClick>((UIVirtualStarmap starmap, int starIndex) =>
            {
                bool pressing = VFInput.rtsConfirm.pressing;
                if ((pressing && !pressSpamProtector) && starIndex == -1)
                {
                    if (starmap.clickText != "" && UIRoot.instance.uiGame.planetDetail.gameObject.activeSelf) // hide planet details
                    {
                        GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/right-group").SetActive(true);
                        UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);
                    }
                    else if (starmap.clickText != "" && !UIRoot.instance.uiGame.planetDetail.gameObject.activeSelf) // hide solar system details
                    {
                        starmap.clickText = "";
                        starmap.OnGalaxyDataReset();
                    }
                    pressSpamProtector = true;
                }
            }));

            return matcher.InstructionEnumeration();
        }

        private static void ClearStarmap(UIVirtualStarmap starmap)
        {
            starmap.starPointBirth.gameObject.SetActive(false);

            foreach (UIVirtualStarmap.StarNode starNode in starmap.starPool)
            {
                starNode.active = false;
                starNode.starData = null;
                starNode.pointRenderer.gameObject.SetActive(false);
                starNode.nameText.gameObject.SetActive(false);
            }
            foreach (UIVirtualStarmap.ConnNode connNode in starmap.connPool)
            {
                connNode.active = false;
                connNode.starA = null;
                connNode.starB = null;
                connNode.lineRenderer.gameObject.SetActive(false);
            }
        }

        private static void ShowSolarSystem(UIVirtualStarmap starmap, int starIndex)
        {
            // start planet compute thread if not done already
            PlanetModelingManager.StartPlanetComputeThread();

            // add star
            StarData starData = starmap._galaxyData.StarById(starIndex + 1); // because StarById() decrements by 1
            AddStarToStarmap(starmap, starData);

            starmap.clickText = starData.id.ToString();
            Log.Debug("Setting it to " + starmap.clickText + " " + starData.id);

            for (int i = 0; i < starData.planetCount; i++)
            {
                // add planets
                PlanetData pData = starData.planets[i];
                Color color = starmap.neutronStarColor;
                float scaleFactor = 0.6f;
                bool isMoon = false;

                VectorLF3 pPos = GetRelativeRotatedPlanetPos(starData, pData, ref isMoon);

                // request generation of planet surface data to display its details when clicked and if not already loaded
                if(!pData.calculated) PlanetModelingManager.RequestCalcPlanet(pData);

                // create fake StarData to pass _OnLateUpdate()
                StarData dummyStarData = new StarData
                {
                    position = pPos,
                    color = starData.color,
                    id = pData.id
                };

                Vector3 scale = (pData.realRadius / 100) * scaleFactor * Vector3.one;
                if(scale.x > 3 || scale.y > 3 || scale.z > 3)
                {
                    scale = new Vector3(3, 3, 3);
                }

                starmap.starPool[i + 1].active = true;
                starmap.starPool[i + 1].starData = dummyStarData;
                starmap.starPool[i + 1].pointRenderer.material.SetColor("_TintColor", color);
                starmap.starPool[i + 1].pointRenderer.transform.localPosition = pPos;
                starmap.starPool[i + 1].pointRenderer.transform.localScale = scale;
                starmap.starPool[i + 1].pointRenderer.gameObject.SetActive(true);
                starmap.starPool[i + 1].nameText.text = pData.displayName + " (" + pData.typeString + ")";
                starmap.starPool[i + 1].nameText.color = Color.Lerp(color, Color.white, 0.5f);
                starmap.starPool[i + 1].nameText.rectTransform.sizeDelta = new Vector2(starmap.starPool[i + 1].nameText.preferredWidth, starmap.starPool[i + 1].nameText.preferredHeight);
                starmap.starPool[i + 1].nameText.rectTransform.anchoredPosition = new Vector2(-2000f, -2000f);
                starmap.starPool[i + 1].textContent = pData.displayName + " (" + pData.typeString + ")";

                starmap.starPool[i + 1].nameText.gameObject.SetActive(true);

                // add orbit renderer
                starmap.connPool[i].active = true;
                starmap.connPool[i].lineRenderer.material.SetColor("_LineColorA", Color.Lerp(color, Color.white, 0.65f));

                if(starmap.connPool[i].lineRenderer.positionCount != 61)
                {
                    starmap.connPool[i].lineRenderer.positionCount = 61;
                }
                for(int j = 0; j < 61; j++)
                {
                    float f = (float)j * 0.017453292f * 6f; // ty dsp devs :D
                    Vector3 cPos = GetCenterOfOrbit(starData, pData, ref isMoon);
                    Vector3 position;
                    if (isMoon)
                    {
                        position = new Vector3(Mathf.Cos(f) * pData.orbitRadius * orbitScaler * 8 + (float)cPos.x, cPos.y, Mathf.Sin(f) * pData.orbitRadius * orbitScaler * 8 + (float)cPos.z);
                    }
                    else
                    {
                        position = new Vector3(Mathf.Cos(f) * pData.orbitRadius * orbitScaler + (float)cPos.x, cPos.y, Mathf.Sin(f) * pData.orbitRadius * orbitScaler + (float)cPos.z);
                    }

                    // rotate position around center by orbit angle
                    Quaternion quaternion = Quaternion.Euler(pData.orbitInclination, pData.orbitInclination, pData.orbitInclination);
                    Vector3 dir = quaternion * (position - cPos);
                    position = dir + cPos;

                    starmap.connPool[i].lineRenderer.SetPosition(j, position);
                }
                starmap.connPool[i].lineRenderer.gameObject.SetActive(true);
            }
        }

        private static VectorLF3 GetCenterOfOrbit(StarData starData, PlanetData pData, ref bool isMoon)
        {
            if(pData.orbitAroundPlanet != null)
            {
                return GetRelativeRotatedPlanetPos(starData, pData.orbitAroundPlanet, ref isMoon);
            }
            isMoon = false;
            return starData.position;
        }

        private static VectorLF3 GetRelativeRotatedPlanetPos(StarData starData, PlanetData pData, ref bool isMoon)
        {
            VectorLF3 pos;
            VectorLF3 dir;
            Quaternion quaternion;
            if (pData.orbitAroundPlanet != null)
            {
                VectorLF3 centerPos = GetRelativeRotatedPlanetPos(starData, pData.orbitAroundPlanet, ref isMoon);
                isMoon = true;
                pos = new VectorLF3(Mathf.Cos(pData.orbitPhase) * pData.orbitRadius * orbitScaler * 8 + centerPos.x, centerPos.y, Mathf.Sin(pData.orbitPhase) * pData.orbitRadius * orbitScaler * 8 + centerPos.z);
                quaternion = Quaternion.Euler(pData.orbitInclination, pData.orbitInclination, pData.orbitInclination);
                dir = quaternion * (pos - centerPos);
                return dir + centerPos;
            }
            pos = new VectorLF3(Mathf.Cos(pData.orbitPhase) * pData.orbitRadius * orbitScaler + starData.position.x, starData.position.y, Mathf.Sin(pData.orbitPhase) * pData.orbitRadius * orbitScaler + starData.position.z);
            quaternion = Quaternion.Euler(pData.orbitInclination, pData.orbitInclination, pData.orbitInclination);
            dir = quaternion * (pos - starData.position);
            return dir + starData.position;
        }

        // probably reverse patch this if there is time
        private static void AddStarToStarmap(UIVirtualStarmap starmap, StarData starData)
        {
            Color color = starmap.starColors.Evaluate(starData.color);
            if (starData.type == EStarType.NeutronStar)
            {
                color = starmap.neutronStarColor;
            }
            else if (starData.type == EStarType.WhiteDwarf)
            {
                color = starmap.whiteDwarfColor;
            }
            else if (starData.type == EStarType.BlackHole)
            {
                color = starmap.blackholeColor;
            }
            float num2 = 1.2f;
            if (starData.type == EStarType.GiantStar)
            {
                num2 = 3f;
            }
            else if (starData.type == EStarType.WhiteDwarf)
            {
                num2 = 0.6f;
            }
            else if (starData.type == EStarType.NeutronStar)
            {
                num2 = 0.6f;
            }
            else if (starData.type == EStarType.BlackHole)
            {
                num2 = 0.8f;
            }
            string text = starData.displayName + "  ";
            if (starData.type == EStarType.GiantStar)
            {
                if (starData.spectr <= ESpectrType.K)
                {
                    text += "红巨星".Translate();
                }
                else if (starData.spectr <= ESpectrType.F)
                {
                    text += "黄巨星".Translate();
                }
                else if (starData.spectr == ESpectrType.A)
                {
                    text += "白巨星".Translate();
                }
                else
                {
                    text += "蓝巨星".Translate();
                }
            }
            else if (starData.type == EStarType.WhiteDwarf)
            {
                text += "白矮星".Translate();
            }
            else if (starData.type == EStarType.NeutronStar)
            {
                text += "中子星".Translate();
            }
            else if (starData.type == EStarType.BlackHole)
            {
                text += "黑洞".Translate();
            }
            else if (starData.type == EStarType.MainSeqStar)
            {
                text = text + starData.spectr.ToString() + "型恒星".Translate();
            }
            if (starData.index == ((customBirthStar != -1) ? customBirthStar - 1 : starmap._galaxyData.birthStarId - 1))
            {
                text = "即将登陆".Translate() + "\r\n" + text;
            }
            starmap.starPool[0].active = true;
            starmap.starPool[0].starData = starData;
            starmap.starPool[0].pointRenderer.material.SetColor("_TintColor", color);
            starmap.starPool[0].pointRenderer.transform.localPosition = starData.position;
            starmap.starPool[0].pointRenderer.transform.localScale = 2 * num2 * Vector3.one;
            starmap.starPool[0].pointRenderer.gameObject.SetActive(true);
            starmap.starPool[0].nameText.text = text;
            starmap.starPool[0].nameText.color = Color.Lerp(color, Color.white, 0.5f);
            starmap.starPool[0].nameText.rectTransform.sizeDelta = new Vector2(starmap.starPool[0].nameText.preferredWidth, starmap.starPool[0].nameText.preferredHeight);
            starmap.starPool[0].nameText.rectTransform.anchoredPosition = new Vector2(-2000f, -2000f);
            starmap.starPool[0].textContent = text;

            starmap.starPool[0].nameText.gameObject.SetActive(true);
        }

        // mark correct star with the '>> Mission start <<' text
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIVirtualStarmap.OnGalaxyDataReset))]
        public static IEnumerable<CodeInstruction> OnGalaxyDataReset_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Translate"),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Concat"),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StarData), "index")),
                    new CodeMatch(OpCodes.Brtrue))
                .Advance(-1)
                .SetAndAdvance(OpCodes.Ldarg_0, null)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<IsBirthStar2>((starData, starmap) =>
                {
                    if(starData == null || starmap == null)
                    {
                        return true;
                    }
                    return starData.index != ((customBirthStar != -1) ? customBirthStar - 1 : starmap._galaxyData.birthStarId - 1);
                }));
            return matcher.InstructionEnumeration();
        }
    }
}
