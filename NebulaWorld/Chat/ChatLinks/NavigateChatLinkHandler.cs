#region

using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat.ChatLinks;

public class NavigateChatLinkHandler : IChatLinkHandler
{
    const char SplitSeparator = '\t';

    public void OnClick(string data)
    {
        var substrings = data.Split(SplitSeparator);
        switch (substrings.Length)
        {
            case 2: // PlayerId
                if (!ushort.TryParse(substrings[0], out var playerId)) return;
                Multiplayer.Session.Gizmos.SetIndicatorPlayerId(playerId);
                ChatManager.Instance.SendChatMessage("Navigate to ".Translate() + substrings[1],
                    ChatMessageType.CommandOutputMessage);
                break;

            case 3: // AstroId
                if (!int.TryParse(substrings[0], out var astroId)) return;
                if (astroId < 0) return;

                // Clear the guide line if choosing the same target
                var previousAstro = GameMain.mainPlayer.navigation.indicatorAstroId;
                GameMain.mainPlayer.navigation.indicatorAstroId = previousAstro != astroId ? astroId : 0;
                break;

            case 5: // PlanetPos
                if (!TryParsePlanetPos(substrings, out var planetId, out var pos)) return;
                Multiplayer.Session.Gizmos.SetIndicatorPing(planetId, pos);
                break;
        }
    }

    public void OnRightClick(string data)
    {
        var gizmoManager = Multiplayer.Session.Gizmos;
        var substrings = data.Split(SplitSeparator);
        switch (substrings.Length)
        {
            case 2: // PlayerId
                if (!ushort.TryParse(substrings[0], out var playerId)) return;
                if (gizmoManager.ObservingPlayerId == playerId)
                {
                    gizmoManager.ObservingPlayerId = 0;
                    return;
                }
                gizmoManager.ObservingPlayerId = playerId;
                ChatManager.Instance.SendChatMessage("Tracking mode (ESC or click again to exit)".Translate(),
                    ChatMessageType.CommandOutputMessage);
                break;

            case 3: // AstroId
                if (!int.TryParse(substrings[0], out var astorId)) return;
                if (astorId <= 0) return;

                // Open the starmap and focus on the target
                UIRoot.instance.uiGame.OpenStarmap();
                var starmap = UIRoot.instance.uiGame.starmap;
                var planet = GameMain.galaxy.PlanetById(astorId);
                var star = GameMain.galaxy.StarById(astorId / 100);
                if (planet != null)
                {
                    var starIdx = astorId / 100 - 1;
                    starmap.focusStar = starmap.starUIs[starIdx];
                    starmap.focusPlanet = null;
                    starmap.OnCursorFunction2Click(0);
                    starmap.screenCameraController.SetViewTarget(planet, null, null, null, VectorLF3.zero,
                        planet.realRadius * 0.00025 * 6.0, planet.realRadius * 0.00025 * 160.0, true, false);
                }
                else if (star != null)
                {
                    var starIdx = astorId / 100 - 1;
                    starmap.focusPlanet = null;
                    starmap.focusStar = starmap.starUIs[starIdx];
                    starmap.OnCursorFunction2Click(0);
                }
                return;

            case 5: // PlanetPos
                if (!TryParsePlanetPos(substrings, out var planetId, out var pos)) return;
                if (gizmoManager.ObservingPlanetId == planetId && gizmoManager.ObservingPos == pos)
                {
                    gizmoManager.ObservingPlanetId = 0;
                    return;
                }
                gizmoManager.ObservingPlanetId = planetId;
                gizmoManager.ObservingPos = pos;
                if (GameCamera.instance.planetMode && planetId == GameMain.localPlanet?.id)
                {
                    // In planet view, move the camera to aim at the target
                    var toRotate = Quaternion.FromToRotation(Vector3.up, pos);
                    var vector = toRotate * Vector3.up;
                    var normalized = Vector3.Cross(vector, Vector3.up).normalized;
                    toRotate = Quaternion.LookRotation(Vector3.Cross(normalized, vector), vector);
                    GameCamera.instance.planetPoser.rotationWanted = toRotate;
                }
                ChatManager.Instance.SendChatMessage("Tracking mode (ESC or click again to exit)".Translate(),
                    ChatMessageType.CommandOutputMessage);
                break;
        }
    }

    public void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        if (!string.IsNullOrEmpty(data))
        {
            UpdateTip(trigger, ref tipObject);
        }
        else if (tipObject is UIButtonTip)
        {
            Object.Destroy(tipObject.gameObject);
        }
    }


    public string GetIconName(string data)
    {
        return string.Empty;
    }

    public string GetDisplayRichText(string data)
    {
        return FormatNavigateString(data);
    }

    private static void UpdateTip(ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        var rect = (RectTransform)trigger.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out var offset);
        offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

        var buttonTip = tipObject as UIButtonTip;
        if (buttonTip == null)
        {
            buttonTip = UIButtonTip.Create(false, "Navigate".Translate(),
                "Left click to create a navigate line to the target.\nRight click to track the target.".Translate(), 2, offset, 0, rect, "", "");
            if (tipObject != null)
            {
                Object.Destroy(tipObject.gameObject);
            }

            tipObject = buttonTip;
        }

        if (!buttonTip.gameObject.activeSelf)
        {
            buttonTip.gameObject.SetActive(true);
            buttonTip.SetTip(false, "Navigate".Translate(), "Left click to create a navigate line to the target.\nRight click to track the target.".Translate(), 2,
                offset, 0, rect, "", "");
        }

        if (buttonTip.isActiveAndEnabled && !buttonTip.titleComp.text.Equals("Navigate"))
        {
            buttonTip.SetTip(false, "Navigate".Translate(), "Left click to create a navigate line to the target.\nRight click to track the target.".Translate(), 2,
                offset, 0, rect, "", "");
        }
    }

    public static string FormatNavigateString(string data)
    {
        var substrings = data.Split(SplitSeparator);
        return $"<link=\"navigate {data}\"><color=\"white\"><u>{substrings[substrings.Length - 1]}</u></color></link>";
    }

    public static string FormatNavigateToPlayerString(ushort playerId, string displayString)
    {
        var data = playerId + SplitSeparator.ToString() + displayString;
        return FormatNavigateString(data);
    }

    public static string FormatNavigateToAstro(int astroId, string displayString)
    {
        var data = astroId + SplitSeparator.ToString()
            + "astroId" + SplitSeparator.ToString() + displayString;
        return FormatNavigateString(data);
    }

    public static string FormatNavigateToPlanetPos(int planetId, Vector3 pos, string displayString)
    {
        var data = planetId + SplitSeparator.ToString()
            + pos.x + SplitSeparator.ToString() + pos.y + SplitSeparator.ToString() + pos.z + SplitSeparator.ToString()
            + displayString;
        return FormatNavigateString(data);
    }

    private static bool TryParsePlanetPos(string[] substrings, out int planetId, out Vector3 pos)
    {
        planetId = 0;
        pos = Vector3.zero;
        if (substrings.Length != 5) return false;
        if (!int.TryParse(substrings[0], out planetId)) return false;
        if (!float.TryParse(substrings[1], out var x)) return false;
        if (!float.TryParse(substrings[2], out var y)) return false;
        if (!float.TryParse(substrings[3], out var z)) return false;
        pos = new Vector3(x, y, z);
        return true;
    }
}
