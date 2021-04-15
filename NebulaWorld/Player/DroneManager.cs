using NebulaModel.Packets.Players;
using System;

namespace NebulaWorld.Player
{
    public static class DroneManager
    {
        public static int[] DronePriorities = new int[255];
        public static Random rnd = new Random();

        public static void BroadcastDroneOrder(int droneId, int entityId, int stage)
        {
            int priority = 0;
            if (stage == 1 || stage == 2)
            {
                priority = rnd.Next();
                DronePriorities[droneId] = priority;
            }
            LocalPlayer.SendPacketToLocalPlanet(new NewDroneOrderPacket(GameMain.mainPlayer.planetId, droneId, entityId, LocalPlayer.PlayerId, stage, priority));
        }    
    }
}
