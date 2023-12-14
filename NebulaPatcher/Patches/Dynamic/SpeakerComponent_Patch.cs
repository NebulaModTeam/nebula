#region

using System;
using HarmonyLib;
using NebulaModel.Packets.Factory.Monitor;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
[HarmonyPatch(typeof(SpeakerComponent))]
internal class SpeakerComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetTone))]
    public static void SetPassColorId_Prefix(MonitorComponent __instance, int __0)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        //Assume the monitor is on local planet
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetTone, __0));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetVolume))]
    public static void SetVolume_Prefix(MonitorComponent __instance, int __0)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetVolume, __0));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetPitch))]
    public static void SetPitch_Prefix(MonitorComponent __instance, int __0)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetPitch, __0));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetLength))]
    public static void SetLength_Prefix(MonitorComponent __instance, float __0)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        var p1 = BitConverter.ToInt32(BitConverter.GetBytes(__0), 0);
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetLength, p1));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetRepeat))]
    public static void SetLength_Prefix(MonitorComponent __instance, bool __0)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetRepeat, __0 ? 1 : 0));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpeakerComponent.SetFalloffRadius))]
    public static void SetFalloffRadius_Prefix(MonitorComponent __instance, float __0, float __1)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMonitorPacket)
        {
            return;
        }
        var planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
        var p1 = BitConverter.ToInt32(BitConverter.GetBytes(__0), 0);
        var p2 = BitConverter.ToInt32(BitConverter.GetBytes(__1), 0);
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SpeakerSettingUpdatePacket(planetId, __instance.id, SpeakerSettingEvent.SetFalloffRadius, p1, p2));
    }
}
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
