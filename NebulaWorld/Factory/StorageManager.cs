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
        public static readonly ToggleSwitch EventFromServer = new ToggleSwitch();
        public static readonly ToggleSwitch EventFromClient = new ToggleSwitch();
        public static bool IsHumanInput = false;
    }
}
