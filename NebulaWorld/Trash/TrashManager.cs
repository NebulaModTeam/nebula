using NebulaModel.DataStructures;
using System;

namespace NebulaWorld.Trash
{
    public class TrashManager : IDisposable
    {
        public readonly ToggleSwitch NewTrashFromOtherPlayers = new ToggleSwitch();
        public readonly ToggleSwitch RemoveTrashFromOtherPlayers = new ToggleSwitch();
        public readonly ToggleSwitch ClearAllTrashFromOtherPlayers = new ToggleSwitch();

        public TrashManager()
        {
        }

        public void Dispose()
        {
        }

        public void SwitchTrashWithIds(int itemId1, int itemId2)
        {
            TrashContainer container = GameMain.data.trashSystem.container;
            TrashObject to = container.trashObjPool[itemId1];
            TrashData td = container.trashDataPool[itemId1];
            container.trashObjPool[itemId1] = container.trashObjPool[itemId2];
            container.trashDataPool[itemId1] = container.trashDataPool[itemId2];
            container.trashObjPool[itemId2] = to;
            container.trashDataPool[itemId2] = td;
        }
    }
}
