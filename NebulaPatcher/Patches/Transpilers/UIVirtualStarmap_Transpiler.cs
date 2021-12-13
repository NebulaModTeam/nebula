using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(UIVirtualStarmap))]
    class UIVirtualStarmap_Transpiler
    {
        private delegate void ShowSolarsystemDetails(UIVirtualStarmap starmap, int starIndex);

        private static float orbitScaler = 5f;
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
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIVirtualStarmap._OnLateUpdate))]
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
                    if (Multiplayer.Session != null && Multiplayer.Session.IsInLobby && starmap.clickText == "")
                    {
                        ClearStarmap(starmap);
                        ShowSolarSystem(starmap, starIndex + 1);
                    }
                }));

            return matcher.InstructionEnumeration();
        }

        private static void ClearStarmap(UIVirtualStarmap starmap)
        {
            starmap.clickText = "blockVanillaStuff";
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
            // add star
            StarData starData = starmap._galaxyData.StarById(starIndex);
            AddStarToStarmap(starmap, starData);

            for (int i = 0; i < starData.planetCount; i++)
            {
                // add planets
                PlanetData pData = starData.planets[i];
                Color color = starmap.neutronStarColor;
                float scaleFactor = 0.6f;
                bool isMoon = false;

                VectorLF3 pPos = GetRelativeRotatedPlanetPos(starData, pData, ref isMoon);

                // create fake StarData to pass _OnLateUpdate()
                StarData dummyStarData = new StarData();
                dummyStarData.position = pPos;
                dummyStarData.color = starData.color;

                starmap.starPool[i + 1].active = true;
                starmap.starPool[i + 1].starData = dummyStarData;
                starmap.starPool[i + 1].pointRenderer.material.SetColor("_TintColor", color);
                starmap.starPool[i + 1].pointRenderer.transform.localPosition = pPos;
                starmap.starPool[i + 1].pointRenderer.transform.localScale = Vector3.one * scaleFactor * (pData.realRadius / 100);
                starmap.starPool[i + 1].pointRenderer.gameObject.SetActive(true);
                starmap.starPool[i + 1].nameText.text = pData.displayName + " (" + pData.typeString + ")";
                starmap.starPool[i + 1].nameText.color = Color.Lerp(color, Color.white, 0.5f);
                starmap.starPool[i + 1].nameText.rectTransform.sizeDelta = new Vector2(starmap.starPool[i + 1].nameText.preferredWidth, starmap.starPool[i + 1].nameText.preferredHeight);
                starmap.starPool[i + 1].nameText.rectTransform.anchoredPosition = new Vector2(-2000f, -2000f);
                starmap.starPool[i + 1].textContent = pData.displayName + " (" + pData.typeString + ")";

                starmap.starPool[i + 1].nameText.gameObject.SetActive(true);
                // if birth planet add renderer to it

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
            if (starData.index == 0)
            {
                text = "即将登陆".Translate() + "\r\n" + text;
            }
            starmap.starPool[0].active = true;
            starmap.starPool[0].starData = starData;
            starmap.starPool[0].pointRenderer.material.SetColor("_TintColor", color);
            starmap.starPool[0].pointRenderer.transform.localPosition = starData.position;
            starmap.starPool[0].pointRenderer.transform.localScale = Vector3.one * num2 * 2;
            starmap.starPool[0].pointRenderer.gameObject.SetActive(true);
            starmap.starPool[0].nameText.text = text;
            starmap.starPool[0].nameText.color = Color.Lerp(color, Color.white, 0.5f);
            starmap.starPool[0].nameText.rectTransform.sizeDelta = new Vector2(starmap.starPool[0].nameText.preferredWidth, starmap.starPool[0].nameText.preferredHeight);
            starmap.starPool[0].nameText.rectTransform.anchoredPosition = new Vector2(-2000f, -2000f);
            starmap.starPool[0].textContent = text;

            starmap.starPool[0].nameText.gameObject.SetActive(true);
        }
    }
}
