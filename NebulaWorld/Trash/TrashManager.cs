namespace NebulaWorld.Trash
{
    public static class TrashManager
    {
        public static bool NewTrashFromOtherPlayers = false;
        public static bool RemoveTrashFromOtherPlayers = false;
        public static bool ClearAllTrashFromOtherPlayers = false;

        public static void SwitchTrashWithIds(int itemId1, int itemId2)
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
