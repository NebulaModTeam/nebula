#region

using System;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld.Trash;

public class TrashManager : IDisposable
{
    public readonly ToggleSwitch ClearAllTrashFromOtherPlayers = new();
    public readonly ToggleSwitch NewTrashFromOtherPlayers = new();
    public readonly ToggleSwitch RemoveTrashFromOtherPlayers = new();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static void SwitchTrashWithIds(int itemId1, int itemId2)
    {
        var container = GameMain.data.trashSystem.container;
        var to = container.trashObjPool[itemId1];
        var td = container.trashDataPool[itemId1];
        container.trashObjPool[itemId1] = container.trashObjPool[itemId2];
        container.trashDataPool[itemId1] = container.trashDataPool[itemId2];
        container.trashObjPool[itemId2] = to;
        container.trashDataPool[itemId2] = td;
    }
}
