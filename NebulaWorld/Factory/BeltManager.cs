using NebulaModel.Packets.Factory.Belt;
using System;
using System.Collections.Generic;

namespace NebulaWorld.Factory
{
    public class BeltManager : IDisposable
    {
        public List<BeltUpdate> BeltUpdates;

        public BeltManager()
        {
            BeltUpdates = new List<BeltUpdate>();
        }

        public void Dispose()
        {
            BeltUpdates = null;
        }

        public void BeltPickupStarted()
        {
            BeltUpdates.Clear();
        }

        public void RegisterBeltPickupUpdate(int itemId, int count, int beltId)
        {
            if (Multiplayer.IsActive)
            {
                BeltUpdates.Add(new BeltUpdate(itemId, count, beltId));
            }
        }

        public void BeltPickupEnded()
        {
            if (GameMain.data.localPlanet != null)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new BeltUpdatePickupItemsPacket(BeltUpdates.ToArray(), GameMain.data.localPlanet.id));
            }

            BeltUpdates.Clear();
        }
    }
}
