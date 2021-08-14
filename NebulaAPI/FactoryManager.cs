// unset

using HarmonyLib;
using System;
using System.Reflection;

namespace NebulaAPI
{
    public static class FactoryManager
    {
        private static Type _factoryManager;
        private static Type _toggleSwitch;

        internal static void Init()
        {
            _factoryManager = AccessTools.TypeByName("NebulaWorld.Factory.FactoryManager");
            _toggleSwitch = AccessTools.TypeByName("NebulaModel.DataStructures.ToggleSwitch");
        }

        public static bool IsIncomingRequest
        {
            get
            {
                object toggle = _factoryManager.GetField("IsIncomingRequest").GetValue(null);
                return (bool) _toggleSwitch.GetProperty("Value").GetValue(toggle);
            }
        }

        public static int PacketAuthor
        {
            get => (int) _factoryManager.GetProperty("PacketAuthor").GetValue(null);
            set => _factoryManager.GetProperty("PacketAuthor").SetValue(null, value);
        }

        public static int TargetPlanet
        {
            get => (int) _factoryManager.GetProperty("TargetPlanet").GetValue(null);
            set => _factoryManager.GetProperty("TargetPlanet").SetValue(null, value);
        }
        
        public static PlanetFactory EventFactory
        {
            get => (PlanetFactory) _factoryManager.GetProperty("EventFactory").GetValue(null);
            set => _factoryManager.GetProperty("EventFactory").SetValue(null, value);
        }
        
        public const int PLANET_NONE = -2;
        public const int AUTHOR_NONE = -1;
        public const int STAR_NONE = -1;

        public static IDisposable IsIncomingRequest_On()
        {
            object toggle = _factoryManager.GetField("IsIncomingRequest").GetValue(null);
            return (IDisposable) _toggleSwitch.GetMethod("On", new Type[0]).Invoke(toggle, new object[0]);
        }

        public static void AddPlanetTimer(int planetId)
        {
            _factoryManager.GetMethod("AddPlanetTimer").Invoke(null, new object[] {planetId});
        }
    }
}