using NebulaModel.DataStructures;

namespace NebulaWorld.Planet
{
    public static class PlanetManager
    {
        public static readonly ToggleSwitch EventFromServer = new ToggleSwitch();
        public static readonly ToggleSwitch EventFromClient = new ToggleSwitch();

        public static void Initialize()
        {
        }
    }
}
