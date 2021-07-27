using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncResponseProcessor : PacketProcessor<StorageSyncResponsePacket>
    {
        public override void ProcessPacket(StorageSyncResponsePacket packet, NebulaConnection conn)
        {
            if (IsHost) return;

            StorageComponent storageComponent = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool[packet.StorageIndex];
            if (storageComponent != null)
            {
                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StorageComponent))
                {
                    storageComponent.Import(reader.BinaryReader);
                }
                ItemProto itemProto = LDB.items.Select((int)GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entityPool[storageComponent.entityId].protoId);

                //Imitation of UIStorageWindow.OnStorageIdChange()
                StorageManager.ActiveWindowTitle.text = itemProto.name;
                StorageManager.ActiveUIStorageGrid._Free();
                StorageManager.ActiveUIStorageGrid._Init(storageComponent);
                StorageManager.ActiveStorageComponent = storageComponent;
                MethodInvoker.GetHandler(AccessTools.Method(typeof(UIStorageGrid), "SetStorageData")).Invoke(StorageManager.ActiveUIStorageGrid, StorageManager.ActiveStorageComponent);
                StorageManager.ActiveUIStorageGrid._Open();
                StorageManager.ActiveUIStorageGrid.OnStorageDataChanged();
                StorageManager.ActiveBansSlider.maxValue = (float)storageComponent.size;
                StorageManager.ActiveBansSlider.value = (float)(storageComponent.size - storageComponent.bans);
                StorageManager.ActiveBansValueText.text = StorageManager.ActiveBansSlider.value.ToString();
                GameMain.galaxy.PlanetById(packet.PlanetId).factory.factoryStorage.storagePool[packet.StorageIndex] = storageComponent;
            }
        }
    }
}