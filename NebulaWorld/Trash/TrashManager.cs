#region

using System;
using NebulaModel.DataStructures;
using UnityEngine;

#endregion

namespace NebulaWorld.Trash;

public class TrashManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();
    public readonly ToggleSwitch RemoveTrashFromOtherPlayers = new();

    public int PlanetId { get; set; }
    public Vector3 LocalPos { get; set; }
    public int ClientTrashCount { get; set; } //Record space and local planet trash count

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static void SetNextTrashId(int trashId)
    {
        var container = GameMain.data.trashSystem.container;
        if (trashId >= container.trashCursor)
        {
            container.trashCursor = trashId;
            while (container.trashCursor >= container.trashCapacity)
            {
                container.SetTrashCapacity(container.trashCapacity * 2);
            }
        }
        else
        {
            container.trashRecycle[0] = trashId;
            container.trashRecycleCursor = 1;
        }
    }

    public void Refresh()
    {
        if (Multiplayer.Session.IsServer) return;

        // When client load or leave planet, refresh the trash count = permanent trash + local planet trash
        ClientTrashCount = 0;
        var localPlanetId = GameMain.localPlanet?.id ?? -1;
        var container = GameMain.data.trashSystem.container;
        var trashObjPool = container.trashObjPool;
        var trashDataPool = container.trashDataPool;
        for (var i = 0; i < container.trashCursor; i++)
        {
            if (trashObjPool[i].item > 0 && (trashDataPool[i].life == 0 || trashDataPool[i].nearPlanetId == localPlanetId))
            {
                ClientTrashCount++;
            }
        }
    }
}
