#region

using HarmonyLib;
using NebulaModel.DataStructures.Chat;
using NebulaWorld;
using NebulaWorld.GameStates;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameCamera))]
public class GameCamera_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameCamera.Logic))]
    public static bool Logic_Prefix()
    {
        // prevent NRE while doing a reconnect as a client issued through the chat command
        return !(GameStatesManager.DuringReconnect && GameMain.mainPlayer == null);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameCamera.Logic))]
    public static void Logic_Postfix(GameCamera __instance)
    {
        if (!Multiplayer.IsActive || GameStatesManager.DuringReconnect) return;

        var observingPlayerId = Multiplayer.Session.Gizmos.ObservingPlayerId;
        if (observingPlayerId == 0) return;

        if (VFInput.escape)
        {
            Multiplayer.Session.Gizmos.ObservingPlayerId = 0;
            ChatManager.Instance.SendChatMessage(
                    "Exit tracking mode".Translate(),
                    ChatMessageType.CommandOutputMessage);
            return;
        }

        if (observingPlayerId > 0)
        {
            TrackingPlayer(__instance, observingPlayerId);
        }
    }

    private static void TrackingPlayer(GameCamera cam, ushort playerId)
    {
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var models))
        {
            if (!models.TryGetValue(playerId, out var model))
            {
                ChatManager.Instance.SendChatMessage(
                    string.Format("Can't find player {0}".Translate(), playerId),
                    ChatMessageType.CommandOutputMessage);
                Multiplayer.Session.Gizmos.ObservingPlayerId = 0;
                return;
            }
            var planetId = model.PlayerInstance.planetId;
            if (planetId > 0 && planetId != GameMain.mainPlayer.planetId)
            {
                ChatManager.Instance.SendChatMessage(
                    string.Format("Player {0} is on a different planet".Translate(), playerId),
                    ChatMessageType.CommandOutputMessage);
                Multiplayer.Session.Gizmos.ObservingPlayerId = 0;
                return;
            }
            cam.rtsTarget.position = model.PlayerTransform.position;
            cam.rtsTarget.eulerAngles = Maths.SphericalRotation(cam.rtsTarget.position, 0f).eulerAngles;
        }
    }
}
