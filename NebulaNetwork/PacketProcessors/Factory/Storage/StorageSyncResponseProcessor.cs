using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    internal class StorageSyncResponseProcessor : PacketProcessor<StorageSyncResponsePacket>
    {
        public override void ProcessPacket(StorageSyncResponsePacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            StorageComponent storageComponent = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool[packet.StorageIndex];
            if (storageComponent != null)
            {
                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StorageComponent))
                {
                    storageComponent.Import(reader.BinaryReader);
                }
                ItemProto itemProto = LDB.items.Select((int)GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entityPool[storageComponent.entityId].protoId);

                //Imitation of UIStorageWindow.OnStorageIdChange()
                Multiplayer.Session.Storage.ActiveWindowTitle.text = itemProto.name;
                Multiplayer.Session.Storage.ActiveUIStorageGrid._Free();
                Multiplayer.Session.Storage.ActiveUIStorageGrid._Init(storageComponent);
                Multiplayer.Session.Storage.ActiveStorageComponent = storageComponent;
                Multiplayer.Session.Storage.ActiveUIStorageGrid.SetStorageData(Multiplayer.Session.Storage.ActiveStorageComponent);
                Multiplayer.Session.Storage.ActiveUIStorageGrid._Open();
                Multiplayer.Session.Storage.ActiveUIStorageGrid.OnStorageDataChanged();
                Multiplayer.Session.Storage.ActiveBansSlider.maxValue = storageComponent.size;
                Multiplayer.Session.Storage.ActiveBansSlider.value = storageComponent.size - storageComponent.bans;
                Multiplayer.Session.Storage.ActiveBansValueText.text = Multiplayer.Session.Storage.ActiveBansSlider.value.ToString();
                GameMain.galaxy.PlanetById(packet.PlanetId).factory.factoryStorage.storagePool[packet.StorageIndex] = storageComponent;
            }
        }
    }
}