#region

using HarmonyLib;
using NebulaModel.Packets.Trash;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(TrashContainer))]
public class TrashContainer_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrashContainer.RemoveTrash))]
    public static bool RemoveTrash_Postfix(TrashContainer __instance, int index)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers)
        {
            return true;
        }

        var itemId = __instance.trashObjPool[index].item;
        var packet = new TrashSystemTrashRemovedPacket(index, itemId);
        if (Multiplayer.Session.IsClient)
        {
            //Set item to 0 to skip in other functions and wait for server approve
            __instance.trashObjPool[index].item = 0;
            Multiplayer.Session.Network.SendPacket(packet);
            return false;
        }

        //For host, broocast to planet if the trash doesn't have a warning attach to it
        if (__instance.trashDataPool[index].nearPlanetId != 0 && __instance.trashDataPool[index].warningId == 0)
        {
            Multiplayer.Session.Network.SendPacketToPlanet(packet, __instance.trashDataPool[index].nearPlanetId);
        }
        else
        {
            Multiplayer.Session.Network.SendPacket(packet);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrashContainer.NewTrash))]
    public static bool NewTrash_Postfix()
    {
        if (Multiplayer.IsActive && Multiplayer.Session.IsClient)
        {
            //Client should wait for server to approve
            return Multiplayer.Session.Trashes.IsIncomingRequest;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TrashContainer.NewTrash))]
    public static void NewTrash_Postfix(ref TrashObject trashObj, ref TrashData trashData, int __result)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.Trashes.IsIncomingRequest)
        {
            if (Multiplayer.Session.Trashes.PlanetId != 0)
            {
                //Partial syncing: create trash only if the client is on the planet
                var planetId = Multiplayer.Session.Trashes.PlanetId;
                trashData.nearPlanetId = planetId;
                trashData.lPos = Multiplayer.Session.Trashes.LocalPos;
                var packet = new TrashSystemNewPlanetTrashPacket(__result, trashObj, trashData);
                if (Multiplayer.Session.IsServer)
                {
                    Multiplayer.Session.Network.SendPacketToPlanet(packet, planetId);
                }
                else
                {
                    Multiplayer.Session.Network.SendPacket(packet);
                }
            }
            else
            {
                Multiplayer.Session.Network.SendPacket(new TrashSystemNewPlayerTrashPacket(
                    Multiplayer.Session.LocalPlayer.Id, __result, trashObj, trashData));
            }
        }
    }
}
