#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

/// <summary>
/// Repairs the Power Dashboard ring graphs in multiplayer sessions.
///
/// Root cause: <c>UISectorGraph</c> binds its data source once, inside <c>_OnInit</c>, and caches
/// <c>fanCount</c> from that array's length. On a client (and when the statistics data arrives
/// after the UI was built) <c>ProductionStatistics</c> reallocates <c>genCapacities</c> /
/// <c>conDemands</c>, leaving the graphs bound to a stale, all-zero array instance.
///
/// The visible symptom is deceptive: the value texts and the right-hand detail lists read the
/// live arrays on every frame, so they are correct, while the rings stay completely invisible
/// because they still draw from the empty old arrays.
///
/// This patch detects that the graphs are pointed at an outdated array and re-runs the game's own
/// <c>_Free</c> / <c>_Init</c> / <c>_Open</c> lifecycle on them, which is exactly how the vanilla
/// UI binds them. No rendering of our own is performed.
/// </summary>
[HarmonyPatch(typeof(UIStatisticsPowerDetailPanel))]
internal class UIStatisticsPowerDetailPanel_Patch
{
    /// <summary>Array instances the graphs were last bound to (see <see cref="RebindIfStale"/>).</summary>
    private static long[] s_boundGenCapacities;
    private static long[] s_boundConDemands;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStatisticsPowerDetailPanel._OnUpdate))]
    private static void _OnUpdate_Postfix(UIStatisticsPowerDetailPanel __instance)
    {
        if (!Multiplayer.IsActive || __instance == null)
        {
            return;
        }

        try
        {
            RebindIfStale(__instance);
        }
        catch (System.Exception e)
        {
            // Never let a cosmetic repair break the statistics window.
            Log.Warn($"Power dashboard rebind failed: {e}");
        }
    }

    private static void RebindIfStale(UIStatisticsPowerDetailPanel panel)
    {
        var production = GameMain.statistics?.production;
        var genCapacities = production?.genCapacities;
        var conDemands = production?.conDemands;
        if (genCapacities == null || conDemands == null)
        {
            return;
        }

        // Cheap per-frame guard: two reference comparisons.
        if (ReferenceEquals(s_boundGenCapacities, genCapacities) &&
            ReferenceEquals(s_boundConDemands, conDemands))
        {
            return;
        }

        s_boundGenCapacities = genCapacities;
        s_boundConDemands = conDemands;

        Rebind(panel.powerGenGraphLarge, genCapacities);
        Rebind(panel.powerConGraphLarge, conDemands);
        Rebind(panel.powerGenGraphSmall, genCapacities);
        Rebind(panel.powerConGraphSmall, conDemands);
    }

    /// <summary>
    /// Replays the vanilla lifecycle so the graph rebinds to the current array.
    /// <c>_Free</c> is required first because <c>_Init</c> deliberately no-ops once inited.
    /// </summary>
    private static void Rebind(UISectorGraph graph, long[] data)
    {
        if (graph == null)
        {
            return;
        }

        graph._Free();
        graph._Init(data);
        graph._Open();
    }
}
