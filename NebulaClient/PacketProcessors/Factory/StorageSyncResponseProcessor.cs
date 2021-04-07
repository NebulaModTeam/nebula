using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class StorageSyncResponseProcessor : IPacketProcessor<StorageSyncResponsePacket>
    {
        public void ProcessPacket(StorageSyncResponsePacket packet, NebulaConnection conn)
        {
            StorageComponent storageComponent = GameMain.data.factories[packet.FactoryIndex]?.factoryStorage?.storagePool[packet.StorageIndex];
            if (storageComponent != null)
            {
                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StorageComponent))
                {
                    storageComponent.Import(reader.BinaryReader);
                }
                ItemProto itemProto = LDB.items.Select((int)GameMain.data.factories[packet.FactoryIndex].entityPool[storageComponent.entityId].protoId);

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
                GameMain.data.factories[packet.FactoryIndex].factoryStorage.storagePool[packet.StorageIndex] = storageComponent;
            }
        }
    }
}