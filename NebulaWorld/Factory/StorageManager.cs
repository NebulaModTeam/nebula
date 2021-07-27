using NebulaModel.DataStructures;
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
        public static readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();
        public static bool IsHumanInput = false;
    }
}
