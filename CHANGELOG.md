## Changelog

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
