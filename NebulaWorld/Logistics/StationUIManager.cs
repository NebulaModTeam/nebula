using NebulaModel.Packets.Logistics;
using NebulaModel.Logger;

namespace NebulaWorld.Logistics
{
    class StationUIManager
    {
        public static void UpdateUI(StationUI packet)
        {
            if (packet.isStorageUI)
            {
                UpdateStorageUI(packet);
            }
            else
            {
                UpdateSettingsUI(packet);
            }
        }

        private static void UpdateSettingsUI(StationUI packet)
        {
            if(packet.settingIndex == 0)
            {

            }
        }

        private static void UpdateStorageUI(StationUI packet)
        {
            PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
            /*
            for (int i = 0; i < GameMain.data.galacticTransport.stationPool.Length; i++)
            {
                // there are a bunch of null ones in there
                if (GameMain.data.galacticTransport.stationPool[i]?.id == packet.stationId)
                {
                    planetId = GameMain.data.galacticTransport.stationPool[i].planetId;
                }
            }
            */
            // if we did not find a corresponding station we exit (should only happen for clients that have not received any transporting or did not visit that planet)
            // TODO: call PlanetTransport::NewStationComponent() for clients when we add one, else PlanetTransport::GetStationComponent() will not be able to find it
            // NOTE: PlanetTransport::NewStationComponent() will also call GalacticTransport::AddStationComponent()

            if (pData == null)
            {
                // this should never happen
                return;
            }
            if (pData.factory == null && !LocalPlayer.IsMasterClient)
            {
                //TODO: now we need to manually do what PlanetTransport::SetStationStorage does because we cant access it through pData.factory.transport
            }
            else
            {
                LocalPlayer.PatchLocks["PlanetTransport"] = true;
                pData.factory.transport.SetStationStorage(packet.stationId, packet.storageIdx, packet.itemId, packet.itemCountMax, packet.localLogic, packet.remoteLogic, null);
                LocalPlayer.PatchLocks["PlanetTransport"] = false;
            }
        }
    }
}
