#region

using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Combat.DFHive;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SpaceSector))]
internal class SpaceSector_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.TryCreateNewHive))]
    public static bool TryCreateNewHive_Prefix(StarData star)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        if (star != null)
        {
            Multiplayer.Session.Network.SendPacket(new DFHiveCreateNewHivePacket(star.id));
        }
        return true;
    }
}
