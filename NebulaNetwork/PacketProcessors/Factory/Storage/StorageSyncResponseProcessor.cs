#region

using System.Globalization;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
internal class StorageSyncResponseProcessor : PacketProcessor<StorageSyncResponsePacket>
{
    protected override void ProcessPacket(StorageSyncResponsePacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        var storageComponent = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage
            ?.storagePool[packet.StorageIndex];
        if (storageComponent == null)
        {
            return;
        }
        using (var reader = new BinaryUtils.Reader(packet.StorageComponent))
        {
            storageComponent.Import(reader.BinaryReader);
        }
        var protoId = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory
            ?.entityPool[storageComponent.entityId].protoId;
        if (protoId != null)
        {
            var itemProto = LDB.items.Select((int)protoId);

            //Imitation of UIStorageWindow.OnStorageIdChange()
            Multiplayer.Session.Storage.ActiveWindowTitle.text = itemProto?.name ?? "";
        }
        Multiplayer.Session.Storage.ActiveUIStorageGrid._Free();
        Multiplayer.Session.Storage.ActiveUIStorageGrid._Init(storageComponent);
        Multiplayer.Session.Storage.ActiveStorageComponent = storageComponent;
        Multiplayer.Session.Storage.ActiveUIStorageGrid.SetStorageData(Multiplayer.Session.Storage.ActiveStorageComponent);
        Multiplayer.Session.Storage.ActiveUIStorageGrid._Open();
        Multiplayer.Session.Storage.ActiveUIStorageGrid.OnStorageDataChanged();
        Multiplayer.Session.Storage.ActiveBansSlider.maxValue = storageComponent.size;
        Multiplayer.Session.Storage.ActiveBansSlider.value = storageComponent.size - storageComponent.bans;
        Multiplayer.Session.Storage.ActiveBansValueText.text =
            Multiplayer.Session.Storage.ActiveBansSlider.value.ToString(CultureInfo.InvariantCulture);
        GameMain.galaxy.PlanetById(packet.PlanetId).factory.factoryStorage.storagePool[packet.StorageIndex] =
            storageComponent;
    }
}
