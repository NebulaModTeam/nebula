using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NebulaWorld.Player
{
    public static class DroneManager
    {
        public static void BroadcastDroneOrder(int droneId, int entityId)
        {
            UnityEngine.Debug.Log($"Drone {droneId} is going to {entityId}");
            LocalPlayer.SendPacketToLocalPlanet(new NewDroneOrderPacket());
        }
    }
}
