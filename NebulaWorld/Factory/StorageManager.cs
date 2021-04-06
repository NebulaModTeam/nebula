using UnityEngine.UI;

namespace NebulaWorld.Factory
{
    public static class StorageManager
    {
        public static StorageComponent ActiveStorageComponent;
        public static UIStorageGrid ActiveUIStorageGrid;
        public static Text ActiveWindowTitle;
        public static Slider ActiveBansSlider;
        public static Text ActiveBansValueText;

        public static bool WindowOpened = false;
        public static bool EventFromServer = false;
        public static bool EventFromClient = false;
        public static bool IsHumanInput = false;
    }
}
