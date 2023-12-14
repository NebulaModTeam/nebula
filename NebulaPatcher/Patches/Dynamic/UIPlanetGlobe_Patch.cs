#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIPlanetGlobe))]
internal class UIPlanetGlobe_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPlanetGlobe.OnNameInputEndEdit))]
    public static void OnNameInputEndEdit_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return;
        }
        if (GameMain.localPlanet != null && !string.IsNullOrEmpty(GameMain.localPlanet.overrideName))
        {
            // Send packet with new planet name
            Multiplayer.Session.Network.SendPacket(new NameInputPacket(GameMain.localPlanet.overrideName,
                NebulaModAPI.STAR_NONE, GameMain.localPlanet.id));
        }
        else if (GameMain.localStar != null && !string.IsNullOrEmpty(GameMain.localStar.overrideName))
        {
            // Send packet with new star name
            Multiplayer.Session.Network.SendPacket(new NameInputPacket(GameMain.localStar.overrideName,
                GameMain.localStar.id, NebulaModAPI.PLANET_NONE));
        }
    }
}
