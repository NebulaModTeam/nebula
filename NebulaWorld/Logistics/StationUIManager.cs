#region

using System;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Logistics;

#endregion

namespace NebulaWorld.Logistics;

public class StationUIManager : IDisposable
{
    public bool
        UIRequestedShipDroneWarpChange
    {
        get;
        set;
    } // when receiving a ship, drone or warp change only take/add items from the one issuing the request

    public StationUI SliderBarPacket { get; set; } // store the change of slider bar temporary, only send it when mouse button is released.

    public int StorageMaxChangeId
    {
        get;
        set;
    } // index of the storage that its slider value changed by the user. -1: None, -2: Syncing

    public ToggleSwitch IsIncomingRequest { get; set; } = new();

    public StationComponent DummyStationStoreContainer { get; private set; } // For UIControlPanelStorageItem

    public StationUIManager()
    {
        DummyStationStoreContainer = new()
        {
            storage = new StationStore[5]
        };
    }

    public void Dispose()
    {
        DummyStationStoreContainer = null;
        GC.SuppressFinalize(this);
    }

    public static void UpdateStation(ref StationUI packet)
    {
        var stationComponent = GetStation(packet.PlanetId, packet.StationId, packet.StationGId);
        if (stationComponent == null)
        {
            return;
        }
        UpdateSettingsUI(stationComponent, ref packet);
        RefreshWindow(packet.PlanetId, packet.StationId);
    }

    public void UpdateStorage(StorageUI packet)
    {
        var stationComponent = GetStation(packet.PlanetId, packet.StationId, packet.StationGId);
        if (stationComponent?.storage == null || stationComponent.storage.Length == 0)
        {
            return;
        }
        UpdateStorageUI(stationComponent, packet);
        RefreshWindow(packet.PlanetId, packet.StationId);
    }

    private static void RefreshWindow(int planetId, int stationId)
    {
        // If station window is opened and viewing the updating station, refresh the window.
        var stationWindow = UIRoot.instance.uiGame.stationWindow;
        if (stationWindow != null && stationWindow.active &&
            stationWindow.factory?.planetId == planetId && stationWindow.stationId == stationId)
        {
            using (Multiplayer.Session.StationsUI.IsIncomingRequest.On())
            {
                stationWindow.OnStationIdChange();
            }
        }

        // If station inspector in control panel is opened and viewing the updating station, refresh the inspector.
        var inspector = UIRoot.instance.uiGame.controlPanelWindow.stationInspector;
        if (inspector != null && inspector.active &&
            inspector.factory?.planetId == planetId && inspector.stationId == stationId)
        {
            using (Multiplayer.Session.StationsUI.IsIncomingRequest.On())
            {
                inspector.OnStationIdChange();
            }
        }
    }

    /**
     * Updates to a given station that should happen in the background.
     * <exception cref="ArgumentOutOfRangeException"></exception>
     */
    private static void UpdateSettingsUI(StationComponent stationComponent, ref StationUI packet)
    {
        // SetDroneCount, SetShipCount may change packet.SettingValue
        switch (packet.SettingIndex)
        {
            case StationUI.EUISettings.MaxChargePower:
                {
                    var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                    if (planet?.factory?.powerSystem?.consumerPool != null)
                    {
                        var consumerPool = planet.factory.powerSystem.consumerPool;
                        if (consumerPool.Length > stationComponent.pcId)
                        {
                            consumerPool[stationComponent.pcId].workEnergyPerTick = (long)(50000.0 * packet.SettingValue + 0.5);
                        }
                    }
                    break;
                }
            case StationUI.EUISettings.SetDroneCount:
                {
                    // Check if new setting is acceptable
                    var maxDroneCount = stationComponent.workDroneDatas?.Length ?? 0;
                    if (maxDroneCount == 0)
                    {
                        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                        var itemProto = LDB.items.Select(planet.factory.entityPool[stationComponent.entityId].protoId);
                        maxDroneCount = itemProto?.prefabDesc.stationMaxDroneCount ?? 10;
                    }
                    var totalCount = Math.Min((int)packet.SettingValue, maxDroneCount);
                    stationComponent.idleDroneCount = Math.Max(totalCount - stationComponent.workDroneCount, 0);
                    totalCount = stationComponent.idleDroneCount + stationComponent.workDroneCount;
                    if (totalCount < (int)packet.SettingValue && packet.ShouldRefund)
                    {
                        // The result is less than original setting, refund extra drones to author
                        var refund = (int)packet.SettingValue - totalCount;
                        GameMain.mainPlayer.TryAddItemToPackage(5001, refund, 0, true);
                    }
                    packet.SettingValue = totalCount;
                    break;
                }
            case StationUI.EUISettings.SetShipCount:
                {
                    // Check if new setting is acceptable
                    var maxShipCount = stationComponent.workShipDatas?.Length ?? 0;
                    if (maxShipCount == 0)
                    {
                        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                        var itemProto = LDB.items.Select(planet.factory.entityPool[stationComponent.entityId].protoId);
                        maxShipCount = itemProto?.prefabDesc.stationMaxShipCount ?? 10;
                    }
                    var totalCount = Math.Min((int)packet.SettingValue, maxShipCount);
                    stationComponent.idleShipCount = Math.Max(totalCount - stationComponent.workShipCount, 0);
                    totalCount = stationComponent.idleShipCount + stationComponent.workShipCount;
                    if (totalCount < (int)packet.SettingValue && packet.ShouldRefund)
                    {
                        // The result is less than original setting, refund extra ships to author
                        var refund = (int)packet.SettingValue - totalCount;
                        GameMain.mainPlayer.TryAddItemToPackage(5002, refund, 0, true);
                    }
                    packet.SettingValue = totalCount;
                    break;
                }
            case StationUI.EUISettings.SetWarperCount:
                {
                    stationComponent.warperCount = (int)packet.SettingValue;
                    if (stationComponent.storage.Length != 0 && packet.WarperShouldTakeFromStorage)
                    {
                        for (var i = 0; i < stationComponent.storage.Length; i++)
                        {
                            if (stationComponent.storage[i].itemId != 1210 || stationComponent.storage[i].count <= 0)
                            {
                                continue;
                            }
                            stationComponent.storage[i].count--;
                            break;
                        }
                    }
                    break;
                }
            case StationUI.EUISettings.MaxTripDrones:
                {
                    stationComponent.tripRangeDrones = Math.Cos(packet.SettingValue / 180.0 * 3.141592653589793);
                    break;
                }
            case StationUI.EUISettings.MaxTripVessel:
                {
                    var value = packet.SettingValue;
                    value = value switch
                    {
                        > 40.5 => 10000.0,
                        > 20.5 => value * 2f - 20f,
                        _ => value
                    };
                    stationComponent.tripRangeShips = 2400000.0 * value;
                    break;
                }
            case StationUI.EUISettings.MinDeliverDrone:
                {
                    var value = (int)(packet.SettingValue * 10f + 0.5f);
                    stationComponent.deliveryDrones = value < 1 ? 1 : value;
                    break;
                }
            case StationUI.EUISettings.MinDeliverVessel:
                {
                    var value = (int)(packet.SettingValue * 10f + 0.5f);
                    stationComponent.deliveryShips = value < 1 ? 1 : value;
                    break;
                }
            case StationUI.EUISettings.WarpDistance:
                {
                    var value = packet.SettingValue;
                    switch (value)
                    {
                        case < 1.5:
                            value = 0.2;
                            break;
                        case < 7.5:
                            value = value * 0.5 - 0.5;
                            break;
                        case < 16.5:
                            value -= 4f;
                            break;
                        case < 20.5:
                            value = value * 2f - 20f;
                            break;
                        default:
                            value = 60;
                            break;
                    }
                    stationComponent.warpEnableDist = 40000.0 * value;
                    break;
                }
            case StationUI.EUISettings.WarperNeeded:
                {
                    stationComponent.warperNecessary = !stationComponent.warperNecessary;
                    break;
                }
            case StationUI.EUISettings.IncludeCollectors:
                {
                    stationComponent.includeOrbitCollector = !stationComponent.includeOrbitCollector;
                    break;
                }
            case StationUI.EUISettings.PilerCount:
                {
                    stationComponent.pilerCount = (int)packet.SettingValue;
                    break;
                }
            case StationUI.EUISettings.MaxMiningSpeed:
                {
                    var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
                    if (factory != null)
                    {
                        var speed = 10000 + (int)(packet.SettingValue + 0.5f) * 1000;
                        factory.factorySystem.minerPool[stationComponent.minerId].speed = speed;
                    }
                    break;
                }
            case StationUI.EUISettings.DroneAutoReplenish:
                {
                    stationComponent.droneAutoReplenish = packet.SettingValue != 0;
                    break;
                }
            case StationUI.EUISettings.ShipAutoReplenish:
                {
                    stationComponent.shipAutoReplenish = packet.SettingValue != 0;
                    break;
                }
            case StationUI.EUISettings.RemoteGroupMask:
                {
                    stationComponent.remoteGroupMask = BitConverter.DoubleToInt64Bits(packet.SettingValue);
                    break;
                }
            case StationUI.EUISettings.RoutePriority:
                {
                    stationComponent.routePriority = (ERemoteRoutePriority)packet.SettingValue;
                    break;
                }
            case StationUI.EUISettings.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Unknown setting index: " + packet.SettingIndex);
        }
    }

    private static StationComponent GetStation(int planetId, int stationId, int stationGid)
    {
        var planet = GameMain.galaxy.PlanetById(planetId);

        // If we can't find planet or the factory for said planet, we can just skip this
        if (planet?.factory?.transport == null)
        {
            return null;
        }

        // Get the station from stationId on the planet
        var stationPool = planet.factory.transport.stationPool;
        var stationComponent = stationPool[stationId];
        var _ = stationGid; // Should ILS be dealt with differently?

        return stationComponent;
    }

    private void UpdateStorageUI(StationComponent stationComponent, StorageUI packet)
    {
        using (Multiplayer.Session.Ships.PatchLockILS.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            switch (packet.ItemCount)
            {
                case -1:
                    planet.factory.transport.SetStationStorage(stationComponent.id, packet.StorageIdx, packet.ItemId,
                        packet.ItemCountMax, packet.LocalLogic, packet.RemoteLogic,
                        packet.ShouldRefund ? GameMain.mainPlayer : null);
                    StorageMaxChangeId = -1;
                    break;
                case -2:
                    stationComponent.storage[packet.StorageIdx].keepMode = packet.KeepMode;
                    break;
                default:
                    stationComponent.storage[packet.StorageIdx].count = packet.ItemCount;
                    stationComponent.storage[packet.StorageIdx].inc = packet.ItemInc;
                    break;
            }
        }
    }
}
