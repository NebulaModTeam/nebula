using HarmonyLib;
using System;

namespace NebulaAPI
{
    public static class LocalPlayer
    {
        private static Type _localPlayer;

        internal static void Init()
        {
            _localPlayer = AccessTools.TypeByName("NebulaWorld.LocalPlayer");
        }

        public static bool IsMasterClient
        {
            get
            {
                if (!NebulaModAPI.nebulaIsInstalled) return true;
            
                return (bool)_localPlayer.GetProperty("IsMasterClient").GetValue(null);
            }
        }

        public static ushort PlayerId
        {
            get
            {
                if (!NebulaModAPI.nebulaIsInstalled) return 0;
            
                return (ushort) _localPlayer.GetProperty("PlayerId").GetValue(null);
            }
        }
        
        
        public static void SendPacket<T>(T packet) where T : class, new()
        {
            if (!NebulaModAPI.nebulaIsInstalled) return;
            
            _localPlayer.GetMethod("SendPacket").Invoke(null, new object[] {packet});
        }

        public static void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            if (!NebulaModAPI.nebulaIsInstalled) return;
            
            _localPlayer.GetMethod("SendPacketToLocalStar").Invoke(null, new object[] {packet});
        }

        public static void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            if (!NebulaModAPI.nebulaIsInstalled) return;
            
            _localPlayer.GetMethod("SendPacketToLocalPlanet").Invoke(null, new object[] {packet});
        }

        public static void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            if (!NebulaModAPI.nebulaIsInstalled) return;
            
            _localPlayer.GetMethod("SendPacketToPlanet").Invoke(null, new object[] {packet, planetId});
        }

        public static void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            if (!NebulaModAPI.nebulaIsInstalled) return;
            
            _localPlayer.GetMethod("SendPacketToStar").Invoke(null, new object[] {packet, starId});
        }
    }
}