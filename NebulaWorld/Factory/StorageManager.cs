using NebulaModel.DataStructures;
using System;
using UnityEngine.UI;

namespace NebulaWorld.Factory
{
    public class StorageManager : IDisposable
    {
        public StorageComponent ActiveStorageComponent;
        public UIStorageGrid ActiveUIStorageGrid;
        public Text ActiveWindowTitle;
        public Slider ActiveBansSlider;
        public Text ActiveBansValueText;
        public bool WindowOpened = false;
        public bool IsHumanInput = false;

        public readonly ToggleSwitch IsIncomingRequest;

        public StorageManager()
        {
            IsIncomingRequest = new ToggleSwitch();
        }

        public void Dispose()
        {
        }
    }
}
