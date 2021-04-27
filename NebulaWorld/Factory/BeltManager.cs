using HarmonyLib;
using NebulaModel.Packets.Belt;
using System.Collections.Generic;

namespace NebulaWorld.Factory
{
    public static class BeltManager
    {
        public static List<BeltUpdate> BeltUpdates = new List<BeltUpdate>();

        public static void BeltPickupStarted()
        {
            BeltUpdates.Clear();
        }
        public static void RegisterBeltPickupUpdate(int itemId, int count, int beltId, int segId)
        {
            if (SimulatedWorld.Initialized)
            {
                BeltUpdates.Add(new BeltUpdate(itemId, count, beltId, segId));
            }
        }
        public static void BeltPickupEnded()
        {
            if (GameMain.data.localPlanet != null)
            {
                LocalPlayer.SendPacketToLocalStar(new BeltUpdatePickupItemsPacket(BeltUpdates.ToArray(), GameMain.data.localPlanet.factoryIndex));
            }
            BeltUpdates.Clear();
        }
    }
}
