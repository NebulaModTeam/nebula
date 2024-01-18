#region

using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(Mecha))]
internal class Mecha_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Mecha.GenerateEnergy))]
    public static bool Mecha_GenerateEnergy_Prefix(Mecha __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        // some players managed to break the fuel chamber on clients.
        // the game thought there is still fuel burning while not adding energy to the mecha and preventing new fuel from beeing added.
        // this checks for this corner case and resets the reactor energy to 0 (empty fuel chamber as displayed to the player)
        if (!Multiplayer.Session.LocalPlayer.IsHost && __instance.reactorEnergy > 0 && __instance.reactorItemId == 0)
        {
            __instance.reactorEnergy = 0;
        }
        return true;
    }

    // We can't do this as client as we won't be able to get_nearestPlanet() since we do not currently have all of the factory info
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddProductionStat))]
    public static bool AddProductionStat_Prefix(int itemId, int itemCount)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        // Send packet to host to add stat
        Multiplayer.Session.Network.SendPacket(new PlayerMechaStat(itemId, itemCount));
        return false;
    }

    // We can't do this as client as we won't be able to get_nearestPlanet() since we do not currently have all of the factory info
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddConsumptionStat))]
    public static bool AddConsumptionStat_Prefix(int itemId, int itemCount)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        // Use negative itemCount to indicate that it is consumption stat
        Multiplayer.Session.Network.SendPacket(new PlayerMechaStat(itemId, -itemCount));
        return false;
    }
}
