#region

using System;
using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(VFAudio))]
internal class VFAudio_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VFAudio.Play), new Type[] { })]
    public static bool Play_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Only play other player sound if the settings is enabled and the event is on the same planet
        if (Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            var onLocalPlanet = Multiplayer.Session.Factories.TargetPlanet == GameMain.localPlanet?.id;
            var fromSelf = Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id;
            return onLocalPlanet && (fromSelf || Config.Options.EnableOtherPlayerSounds);
        }
        if (Multiplayer.Session.Planets.IsIncomingRequest.Value)
        {
            var onLocalPlanet = Multiplayer.Session.Planets.TargetPlanet == GameMain.localPlanet?.id;
            return onLocalPlanet;
        }
        return true;
    }
}
