#region

using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIAstroTodoPanel))]
internal class UIAstroTodoPanel_Patch
{
    /// <summary>
    /// Allow viewing planet memos for unvisited planets by searching global todos pool.
    /// Game returns early if factory is null, but multiplayer syncs todos globally.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAstroTodoPanel.SetData))]
    public static void SetData_Postfix(UIAstroTodoPanel __instance, int _astroId)
    {
        if (!Multiplayer.IsActive) return;

        // If todo is set AND matches this planet, factory was loaded correctly
        // Game's SetData doesn't clear todo when factory is null, so check ownerId
        if (__instance.todo != null && __instance.todo.ownerId == _astroId) return;

        // Not a planet (stars have astroId % 100 == 0)
        if (_astroId % 100 <= 0) return;

        // Factory not loaded - search global todos pool
        var todos = GameMain.data?.galacticDigital?.todos;
        if (todos == null) return;

        for (int i = 1; i < todos.cursor; i++)
        {
            ref var todo = ref todos.buffer[i];
            if (todo.id == i && todo.ownerId == _astroId &&
                todo.ownerType == ETodoModuleOwnerType.Astro)
            {
                __instance.todo = todo;
                __instance.delayLabelUpdate = true;
                break;
            }
        }
    }

    /// <summary>
    /// Sync memo content when user finishes editing.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAstroTodoPanel.OnTodoInputFieldEndEdit))]
    public static void OnTodoInputFieldEndEdit_Postfix(UIAstroTodoPanel __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket) return;
        if (__instance.astroId <= 0 || __instance.todo == null) return;

        Multiplayer.Session.Network.SendPacket(
            new PlanetMemoUpdatePacket(
                __instance.astroId,
                __instance.todo.content,
                __instance.todo.hasReminder,
                __instance.todo.contentColorIndex));
    }

    /// <summary>
    /// Sync reminder checkbox toggle.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAstroTodoPanel.OnCheckBoxButtonClick))]
    public static void OnCheckBoxButtonClick_Postfix(UIAstroTodoPanel __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket) return;
        if (__instance.astroId <= 0 || __instance.todo == null) return;

        Multiplayer.Session.Network.SendPacket(
            new PlanetMemoUpdatePacket(
                __instance.astroId,
                __instance.todo.content,
                __instance.todo.hasReminder,
                __instance.todo.contentColorIndex));
    }
}
