## Changelog

0.9.16:
- Compatible with game version 0.10.32.25682

0.9.15:
- Compatible with game version 0.10.32.25595
- @starfi5h: Add object protoId check for destruct and upgrade requests

0.9.14:
- @starfi5h: Add `Enable Other Player Sounds` config settings in multiplayer general tab  
- @starfi5h: Fix headless server error
- @starfi5h: When GalacticScale is present, hide New Game (Host) button due to compatibility
- @starfi5h: Fix lastSaveTime initialization and the issue of hearing other player audio on the other planet

0.9.13:
- Compatible with game version 0.10.32.25552
- @starfi5h: Sync ejector auto orbit and blueprint paste foundation
- @starfi5h: Force enable drone and shield burst for client
- Note: The dashboard is not synced. Clients will lose their custom stats when they leave the star system

0.9.12:
- Compatible with game version 0.10.31.24697
- @starfi5h: Client can now use metadata to unlock tech. Sandbox unlock by client is now synced
- @starfi5h: Remove metadata requirement for blueprint tech due to client can't get metadata in MP
- @starfi5h: The right part of logistics control panel (I) is available for loaded planet
- @starfi5h: Sync Reference rate, Import/Export storage, Storage amount in production statistics panel

0.9.11:
- @starfi5h: Fix half-growth dark fog bases keep regenerating
- @starfi5h: Fix combat drones doesn't increase ground base threat
- @starfi5h: Fix errors in NgrokManager.IsNgrokActive and SpaceSector.RemoveEnemyWithComponents

0.9.10:
- @fakeboboliu: Support to connect server with WSS by specifying wss:// in the connect Uri
- @starfi5h: Sync Logistics Control Panel (I) entry list
- @starfi5h: Esc in multiplayer menu can now return to the upper-level
- @starfi5h: (Balance) When player killed, Drop half of item in inventory and increase CD from 1.5s to 5s
- @starfi5h: (Balance) Increase base alert range to player from 90 to 200
- @starfi5h: Space hive threat will now increase correctly when client attack relays by player's fleet
- @starfi5h: DF base/hive will now launch attack at player on remote empty planet too
- @starfi5h: Fix client player can't see the death animation of other clients
- @starfi5h: Fix DF base sometimes can't be destroyed in client
- @starfi5h: (Headless server) Stop relay landing when there are 7 or more working shield generators

0.9.9:
- @starfi5h: Fix multiplayer tab in the option window for DSP v0.10.30.23430
- @starfi5h: Separate error close button (x) and copy button
- @starfi5h: Sync WarningBroadcastData: LandingRelay, ApproachingSeed, BuildingDestroyed and 3 more

0.9.8:
- @AlienXAXS: Added Online Player UI (Backtick key by default)
- @AlienXAXS: Updated Nebula to be compatible with Dyson Sphere Program v0.10.30.23292
- @starfi5h: Temporarily disable Logistics Control Panel (I) interactions
- @starfi5h: Fix a bug that battle notification toggle in multiplayer chat settings has no effect

0.9.7:
- @AlienXAXS: Headless now calculates planetary shields on CPU
- @AlienXAXS: Enemy Relay Direction Sync
- @starfi5h: Sync damage to space enemy by mecha fleet
- @starfi5h: Fix NRE in Bomb_Explosive.TickSkillLogic when other player throwing bomb on other planets
- @starfi5h: Fix error in EnemyUnitComponent.RunBehavior_Defense_Ground after client load factory

0.9.6:
- @AlienXAXS: Fix headless server throwing a small error during boot sequence due to the UI being disabled
- @PhantomGamers: Add additional error description to ngrokmanager
- @starfi5h: Enable Log.Debug messages
- @starfi5h: Fix DF relay landed on planet message in client
- @starfi5h: Fix client's attacks won't increase DF threat when host player is dead
- @starfi5h: Fix ACH_BroadcastStar.OnGameTick error on client
- @starfi5h: Prevent server from sending out construction drones in headless mode

0.9.5:
- @starfi5h: Sync Dark Fog communicator (aggressiveness and truce)
- @starfi5h: Show server last save time in client esc menu
- @starfi5h: Fix veins don't get buried by foundations on remote planets and log error crash

0.9.4:
- Compatible with Steam or Game Pass version 0.10.30.22292
- @PhantomGamers: Prevent errors with Ngrok from crashing the game
- @PhantomGamers: Added error descriptions to Ngrok errors
- @starfi5h: Sync interstellar routes
- @starfi5h: Sync tilted conveyor belts

0.9.3:
- @starfi5h: Change chat message format. Player's name now has an underlined link to navigate
- @starfi5h: Add new config option Chat - Show Timestamp to enable/disable timestamp before the chat message
- @starfi5h: Add new CLI arugment `/newgame-cfg` to load the parameters from the config file `nebulaGameDescSettings.cfg`
- @starfi5h: Add new chat command `/dev`
- @starfi5h: Fix inventory error in client
- @starfi5h: Fix hp bar doesn't vanish after deleting the building when client joins
- @starfi5h: Fix enemies and ILS related errors

0.9.2:
- Compatible with Steam version 0.10.29.22015 or Game Pass version 0.10.29.21943
- @sk7725: Added Korean font and TextMeshPro fallback
- @starfi5h: Add new chat command `/playerdata`
- @starfi5h: Launch construction drones if local player is closer or within 15m
- @starfi5h: Fix error when activating super nova in the turret window
- @starfi5h: Fix desync of dyson sphere when client joins the game
- @phantomgamers: Log expected game version and review code

0.9.1:
- Support combat mode syncing (game version 0.10.29.21950)  
- @starfi5h: Implement basic combat syncing framework
- @starfi5h: Add new config option `EnableBattleMessage` to show battle notifications
- @starfi5h: Add map ping: when chat is open, `Ctrl+Alt+LeftClick` on the planet can create a link in chatbox
- @mmjr, @phantomgamers, @sp00ktober: Review code and provide suggestions

0.9.0:
- Now compatible with Dark Fog game version (DSP 0.10.x). Combat mode is not supported yet.
- @phantomgamers: fix compilation after update and overall fixes/cleanup
- @phantomgamers: fix UIVirtualStarmap patches
- @phantomgamers: reviewing code from other contributers
- @starfi5h: fix runtime issues after the update and overall fixes/cleanup
- @starfi5h: improve UI and Keybinding
- @starfi5h: rework Wireless Power Tower syncing
- @starfi5h: add syncing for Battlefield Analysis Base
- @mmjr: disable dark fog switch in lobby and prevent df enabled saves to be loaded.
- @ajh16: sync dark fog lobby settings
- @highrizk: sync storage filters
- @highrizk: update serializer and fix broken packets
- @highrizk: add serialization support and unit tests for dictionaries
- @zzarek: add turret UI syncing
- @sp00ktober: add syncing for new mecha settings and features
- @sp00ktober: sync mecha and battle base construction drones
- @sp00ktober: overall NRE fixes

0.8.14:
- @starfi5h: Fix mecha animation when player count > 2  
- @starfi5h: Fix UIPerformance save test in multiplayer  
- @starfi5h: Disable build/dismantle sounds when too far away  
- @starfi5h: Convert strings to string.Translate() to enable translation  

<details>
<summary>All changes</summary>

0.8.13:

- @starfi5h: Fix compilation with 0.9.27.15466  
- @starfi5h: Add -newgame launch option for dedicated server   

0.8.12:

- @PhantomGamers: Remove exe targeting to support game pass version  
- @starfi5h: Fix errors about logistic bots  
- @starfi5h: Add -load-latest launch option for dedicated server  

0.8.11:

- @starfi5h: Added support for DSP 0.9.27 along with syncing for the new logistics distribution system
- @starfi5h: Optimized network traffic
- @starfi5h: Dedicated servers will now save when gracefully exited (ctrl+c on the console window)
- @starfi5h: Fix error when sail capacity increases in dedicated server

0.8.10:

- @starfi5h: Fix compilation with 0.9.26.13034
- @starfi5h: Fix a bug that makes advance miner power usage abnormal
- @starfi5h: Add new chat settings NotificationDuration

0.8.9:

- @PhantomGamers: Fixed compilation with 0.9.26
- @starfi5h: Added syncing of all of the new Sandbox features introduced in 0.9.26
- @starfi5h: Fixed bug that caused the host to sink into the ground
- @starfi5h: Increased connection timeout to prevent issues with higher latency connections

0.8.8:

- @starfi5h: Added RemoteAccessPassword setting for servers so that users can authenticate to use admin commands
- @starfi5h: Fixed bugs related to headless servers
- @starfi5h: Chat now stores past commands, accessible with the up and down arrow keys
- @PhantomGamers: Fixed white window popping up while headless server is running
- @starfi5h: Added syncing for Logistic Station names
- @starfi5h: Fixed bug that allowed clients to reduce ILS warp distance below their minimum value
- @starfi5h: Added syncing for Mecha energy production and consumption stats
- @starfi5h: Made it so new clients now join the game with full energy and some warpers if the tech is unlocked
- @starfi5h: Fixed bug when running GalacticScale that caused clients to sink into the ground
- @starfi5h: Removed IP addresses from log output

0.8.7:

- @mmjr-x: Add Upnp/pmp support
- @mmjr-x: Add Ngrok support
- @mmjr-x: Add server password support
- @PhantomGamers: Add Discord rich presence support
- @PhantomGamers: Fix error when unable to obtain documents folder
- @starfish: Add headless server support
- @starfish: Add player connect/disconnect message (can be disabled in config)
- @starfish: Fix NRE when loading a gas giant. Fix planet type mismatch caused by older saves.

0.8.6:

- @starfish: Bugfix regarding NRE exception in UpdateDirtyMeshes()
- @starfish: Bugfix desync issues regarding ILS and PLS
- @starfish: Add milestone syncing
- @sp00ktober: Add gracefull error messages regarding broken traffic monitors and broken compression of factory data
- @sp00ktober: Add a reconnect command to the chat for easy and fast reconnection of clients

0.8.5:

- @starfish: Add Dyson Sphere color syncing
- @starfish: Add syncing for fast insert / fast take out of items to / from buildings
- @starfish: Add syncing for fractionator and power generator product
- @starfish: Save game to Last Quit when exiting multiplayer game.
- @starfish: Fix a bug that would lead to an inserter's filter to not set correctly
- @starfish: Fix planet terrain not synced when client loads a factory.
- @starfish: Fix trash item count incorrect when item count > 256.
- @sp00ktober: UI adjustments to account for the game update
- @sp00ktober: Disable metadata upgrades for clients in tech tree
- @sp00ktober: Add syncing for fast insert / fast take out of items to / from belts

0.8.4:

- @kremnev8: add two new events to Nebula API
- @starfish: fixed issue where client would sometimes be unable to load in while using GalacticScale
- @starfish: custom planet and star names now show up in lobby (not while GalacticScale is active)
- @starfi5h: show correct resource amount in UIPlanetDetail
- @starfi5h: show custom planet and star names in lobby screen
- @starfi5h: the selected starting planet name will now show on the lobby screen
- @starfi5h: fixed issue where the nametag on the minimap wouldn't show up for a client that rejoined

0.8.3:

- @kremnev8: improved ingame chat
- @starfish: added compatibility with BulletTime which enables fluent loading times on Planet and System arrival
- @starfish: bugfix regarding too large dyson sphere data
- @starfish: bugfix regarding reloading of dyson sphere
- @starfish: improved loading of solar systems, this now runs on its own thread
- @starfish: developer commands can now be executed from the ingame chat (using /xconsole [command] )
- @sp00ktober: added tooltips to the Nebula settings
- @sp00ktober: added setting to prevent `System.ObjectDisposedException` errors resulting in random client disconnect
- @sp00ktober: added code to handle IndexOutOfBounds errors when importing PlanetFactory data (very rare issue)
- @sp00ktober: fixed wrong array size for storage and slots in ILS
- @sp00ktober: added minimap indicator for other players positions (on the same planet)
- @sp00ktober: added chat command to list planets in a system
- @sp00ktober: added chat command to navigate to star, planet or player by name or id

0.8.2:

- @kremnev8: fix issue with EmojiDataManager when a save was loaded multiple times in a row.

0.8.1:

- @starfish: Add copy&close error button
- @starfish: bugfixes regarding dyson sphere editor
- @starfish: bugfix regarding item refund in matrix labs
- @starfish: bugfix regarding the ILS UI
- @starfish: bugfix regarding placement of spraycoaters, traffic monitors and inserters
- @starfish: bugfix regarding drone and ship counts in stations
- @PhantomGamers, @sp00ktober: adjust TCP fragment size for faster data transmission
- @sp00ktober: bugfix regarding players getting stuck with the "player joining" message
- @sp00ktober: bugfix regarding wrong mecha color until hitting "apply" in mecha editor
- @sp00ktober: bugfix regarding ILS ship rendering clientside
- @sp00ktober: add optional soil syncing
- @sp00ktober: add syncing of mecha editor state and items
- @mattsemar, @kremnev8: add in-game chat functionality with commands (open with `Alt` + `~` by default)

0.8.0:

- Now compatible with DSP 0.9.24
- @starfish: Refactoring of the ILS UI making it more stable and accurate
- @starfish: Update Dyson Sphere syncing to match the new features of the game update
- @starfish: Add UPS syncing to the game making the overall game state more accurate
- @starfish: Updates for the proliferator and advanced miner
- @starfish: Bugfix for wrong objId
- @sp00ktober: Rework ILS ship rendering to be more accurate for clients
- @sp00ktober: Rework ILS item adding (into stations) to be more accurate for clients
- @sp00ktober: Bugfixes related to belts placed with a filter set
- @sp00ktober: Add syncing of MechaAppearance
- @sp00ktober: Fixed a bug that would lock the host with the "player joining" message when multiple clients try to join at the same time

0.7.10:

- @starfish: Added WarningSystem syncing
- @PhantomGamers: Fixed case of NRE when arriving on another planet
- @PhantomGamers: Fixed issue where Universe Exploration tech would break while in a multiplayer game

0.7.9:

- @sp00ktober: gracefully tell older nebula versions that there is a mod version missmatch.
- @sp00ktober: fix planet detail ui stuck in lobby mode while in game.
- @starfish: fix jaggy remote player movement

0.7.8:

- @sp00ktober: Added Lobby feature where you can preview solar systems and choose your birth planet.

0.7.7:

- @starfi5h, @PhantomGamers: Fixed issue where research removed by clients would not be synced.

0.7.6:

- @starfi5h: Added syncing of ray receiver output
- @starfi5h: Fixed lighting of remote players
- @starfi5h: Fixed clients receiving duplicate items when cancelling manual research

0.7.5:

- @sp00ktober: Fixed error caused by warning system introduced in previous update
- @PhantomGamers: Fixed compatibility with DSP 0.8.23.9989

0.7.4:

- @sp00ktober: adjusted mod to be compatible with game version 0.8.23

0.7.3:

- @PhantomGamers: Fixed error when upgrading blueprint previews.
- @sp00ktober: Added hotfix to prevent error caused by ILS ships

0.7.2:

- @sp00ktober: Fixed issue where the host would render buildings placed by players on other planets on his current planet.

0.7.1:

- @starfi5h: Fixed research desync issues
- @sp00ktober: Fixed error when client upgrades buildings on different planet from the host.
- @PhantomGamers: Fixed compatibility with DSP 0.8.22.9331+

0.7.0:

- @phantomgamers: Fixed instance where error would trigger by loading saves made on earlier Nebula versions. **WARNING: All previous client inventory and position data will be lost!** (should be for the last time!)
- @phantomgamers: Fixed error that was triggered by the client loading a planet after traveling to a different planetary system
- @phantomgamers: Fixed error that was triggered by the client warping outside of a planetary system
- @starfi5h: Added syncing of solar sails and rockets when client does not have the planet they originated from loaded.
- @sp00ktober: Implemented smooth loading of factories for clients (fixed clients phasing through planet when flying too fast)

0.6.2:

- Fixed error when loading saves that were created before 0.6.0. **WARNING: All previous client inventory and position data will be lost!**
- Improved compatibility with GigaStations mod (thanks to @kremnev8)
- Removed extraneous dlls that were mistakenly included in the previous release
- Now supports DSP version 0.8.22.8915+ (thanks to @starfi5h!)

0.6.1:

- Fixed statistics syncing (thanks to @starfi5h)
- Fixed audio playing for all players when pasting building settings and warping (thanks to @starfi5h)
- Added syncing for footstep and landing sounds (thanks to @starfi5h)

0.6.0:

- Fixed cases where a multiplayer session could hang on the player joining screen.
- Fixed issue where foundations built by clients would not sync to other clients.
- Fixed issue where the user would not be informed if they were kicked due to a mod mismatch.
- Enabled pausing in Multiplayer when no clients are connected. (thanks to @starfi5h)
- Now supports DSP version 0.8.21.8562+ (also thanks to @starfi5h!)
- Mecha color configuration has been removed from the options in favor of the new option in the Mecha panel

0.5.0:

- Added API that enables other mods to sync over multiplayer! (Big thanks to @kremnev8!)
- Fixed a bug that caused sorters to break when a client built a belt under preexisting sorters.
- Fixed a bug that resulted in the client getting an error after disconnecting from a game that the host left.
- Refactored session architecture (big changes to codebase but should be seamless to users)

0.4.0:

- Nebula now supports DSP version 0.8.20.7962+

0.3.1:

- Fixed issue where if client didn't have enough items to upgrade, the buildings would still be upgraded for the host.
- Clients will now retain their detail display settings between sessions (e.g. power grid visibility) (thanks to @Needix)
- Fixed issue where players would be able to construct buildings made with blueprints even if they did not have the required items.
- Fixed miscellaneous issues related to ILS ship movement
- Fixed error related to host building foundations while on a different planet from the client

0.3.0:

- Added support for blueprint update (0.8.x)
- Improved player name tag rendering
- Fixed newly introduced multithread issues

0.2.0:

- initial release on thunderstore

</details>
