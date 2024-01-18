## Changelog

2.0.0:

- Remove `IPlayerManager`. Use the new class `ConcurrentPlayerCollection` instead.
- Add `IsDedicated`, `IsClient`, `IsServer` properties in `IMultiplayerSession`
- Add `SendToMatching` method in `INetworkProvider`
- Add `ConstructionModule`, `FightData`, `UpdateMech` in `IMechaData`
- Add combat-related upgrade settings in `IPlayerTechBonuses`

<details>
<summary>Previous Changelog</summary>

1.3.1:

- Added DeliveryPackage to IMechaData and IPlayerTechBonuses

1.3.0:

- Add a new event OnDysonSphereLoadFinished to NebulaModAPI
- Add SendPacketExclude<T>() to INetworkProvider
- Add GetPlayerById() to IPlayerManager
- Add SendPacketToOtherPlayers<T>() to IPlayerManager
- Add IEquatable interface to INebulaConnection, now it can use Equals() to test value equality.
- Remove Float4[] MechaColors in IPlayerData

1.2.0:

- @kremnev8: add two new events to Nebula API for players joining and leaving the game

1.1.4:

- Added DIY Mecha Appearance to the IPlayerData interface representing the current state of the mecha editor UI.

1.1.3:

- Added new MechaAppearance to the IPlayerData interface.

1.1.2:

- Bump version for nuget package.

1.1.1:

- Removed extraneous dll that was mistakenly included in the previous release

1.1.0:

- Float3 IPlayerData.MechaColor has been changed to Float4[] IPlayerData.MechaColors in line with changes introduced in
  DSP version 0.8.21.8562.
- Edited description.

1.0.0:

- initial release on thunderstore

</details>
