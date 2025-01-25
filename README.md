# Nebula Multiplayer Mod [![GitHub Release](https://img.shields.io/github/v/release/NebulaModTeam/nebula)](https://github.com/NebulaModTeam/nebula/releases/latest) [![Nightly Build](https://img.shields.io/badge/nightly-Build-8A2BE2?link=https%3A%2F%2Fnightly.link%2FNebulaModTeam%2Fnebula%2Fworkflows%2Fbuild-winx64%2Fmaster%2Fbuild-artifacts-Release.zip)](https://nightly.link/NebulaModTeam/nebula/workflows/build-winx64/master/build-artifacts-Release.zip) [![Build - Win x64](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml/badge.svg)](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml)

An open-source, multiplayer mod for the game [Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).

## Download Prerelease

[![Build - Win x64](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml/badge.svg?branch=master)](https://nightly.link/NebulaModTeam/nebula/workflows/build-winx64/master/build-artifacts-Release.zip) 
Note: This is the bleeding edge build, for the more stable build see below. You can use this one if you want to try out the latest development builds, which may contain bugs and unfinished work.

You will also need some extra steps to get this installed, see 2nd point in ["How can I play this mod?"](#how-can-i-play-this-mod).

## FAQ

### Where can I get mod support?

Please join our [Discord Server](https://discord.gg/UHeB2QvgDa) and ask your question in the `support` channel. We have a really nice community that will be able to answer your questions.

### How can I play this mod?

Please do keep in mind that this mod is still in heavy development, it may still contains bugs.

- Stable version of the mod can be downloaded from [Thunderstore](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerMod/) (Recommended).
- If you want to install the latest version of the mod, you can install pre-release versions be following the [installation guide](https://github.com/NebulaModTeam/nebula/wiki/Installation#manual-installation).
- To connect, check [hosting and joining guide](https://github.com/NebulaModTeam/nebula/wiki/Hosting-and-Joining).

### API Documentation

This mod has an API, that makes it easier for other mod developers to make their mods compatible with Nebula. If you are a mod developer and you want your mods to be compatible, follow the instructions [here](https://github.com/NebulaModTeam/nebula/wiki/Nebula-mod-API). Also you can always join our [Discord Server](https://discord.gg/UHeB2QvgDa) for help with using the API.

### Chat 

The chat window can opened/closed using `Alt + Backtick` (configurable in Settings - Multiplayer - Chat). Type `/help` to view all commands. Also in settings is an option to disable the chat window from automatically opening when a message is received.

### What is the current status?

Major refactors will happen while the project grows or game updates. Join the [Discord Server](https://discord.gg/UHeB2QvgDa) if you want to see to latest state of our development. Check [Wiki](https://github.com/NebulaModTeam/nebula/wiki/About-Nebula) for overview of features.  

The multiplayer mod now supports Dark Fog combat mode in the latest game version (0.10.32.x).  
Most of the battle aspects are sync, only few features are still WIP.  

<details>
<summary>List of peace mode syncing features</summary>

- [x] Server / Client communication
- [x] Custom Multiplayer menu in-game
- [x] Player Movement syncing on Planet
- [x] Player Movement syncing in Space
- [x] Player VFX syncing (jetpack, torch, ...)
- [x] Player SFX syncing (footsteps sound, torch sound, ...)
- [x] Players appearances syncing
- [x] Game Time (UPS) syncing
- [x] Universe settings syncing
- [x] Client planet loading from server
- [x] Planet vegetation mining syncing
- [x] Planet resources syncing
- [x] Build preview syncing
- [x] Entity creation syncing
- [x] Entity desctruction syncing
- [x] Entity upgrade syncing
- [x] Dyson spheres syncing
- [x] Researches syncing
- [x] Factories statistics syncing (some new extra info is not sync)
- [x] Containers inventory syncing
- [x] Building Interaction syncing
- [x] Belts interaction syncing (pickup, putdown)
- [x] Trash (dropped items) syncing
- [x] Interstellar Station syncing
- [x] Drones events syncing
- [x] Foundation syncing (terrain deformation)
- [x] Server state persistence
- [x] Power network syncing (charger and request power from dyson sphere)
- [x] Warning alarm syncing
- [x] Broadcast notification syncing (events with guide icon)
- [x] Logistics Control Panel (I) syncing (entry list and detail panel)
- [ ] Goal system (currently not available in client)
- [ ] Custom dashboard (clients will lost their custom stats when they leave the star system)

</details>


<details>
<summary>List of combat mode syncing features</summary>

- [x] Sync settings of new building (BAB, turrets)
- [x] Sync combat settings
- [x] Sync DF ground enemy create/destroy events (factory.enemyPool)
- [x] Sync DF ground units activate/deactivate event 
- [x] Sync DF space enemy create/destroy events (spaceSector.enemyPool)
- [x] Sync DF space units activate/deactivate events
- [x] Sync DF planet base exp level and threat
- [x] Sync DF space hive exp level and threat
- [x] Sync loot and loot filter table
- [x] Sync mecha shooting weapons
- [x] Sync mecha bombing
- [x] Sync mecha death and respawn animation
- [x] Sync mecha personal shield to block projectiles
- [x] Sync DF base awake events (player lock with weapon, player nearby, under attack)
- [x] Sync DF base threat and launch assault event
- [x] Patch DF unit to search for nearest alive mecha (sensor range)
- [x] Patch DF turret to search for nearest alive mecha (attack when within attack range or counterattack)
- [x] Sync the hatred targets changes so DF units are attacking the same target
- [x] Sync building repair drone (imperfect)
- [x] Sync building kill event (server fully authorized)
- [x] Sync building reconstruct event
- [x] Sync DFRelay ArriveBase/ArriveDock/LeaveBase/LeaveDock events
- [x] Sync Remove base pit event
- [x] Sync TryCreateNewHive, DispatchFromHive events
- [x] Sync hive realize and open/close preview events
- [x] Sync DF hive awake events (player lock with weapon, player nearby, under attack)
- [x] Sync DF hive threat level and launch assault event
- [x] Patch DF unit to search for nearest alive mecha (sensor range)
- [x] Patch DF turret to search for nearest alive mecha (attack when within attack range or counterattack)
- [x] Show base/hive/relay invasion events in chat
- [ ] Sync kill stats
- [x] Sync Dark Fog communicator (aggressiveness and truce)
- [ ] Show remote mecha combat drone fleet animation
- [ ] Show remote mecha spacecraft fleet animation
- [ ] Show ground-to-space attacks animation on client for remote planets (missile turrets, plasma cannon)
- [ ] Show space-to-ground attacks animation for remote planets (lancers invading with sweep laser and bomber)

</details>

### How can I contribute?

Please join our [Discord Server](https://discord.gg/UHeB2QvgDa) to ask if someone is already working on the task that you want to do. Once, you are done with your modification, simply submit a pull request. Contribution documentation can be found here: [Wiki](https://github.com/NebulaModTeam/nebula/wiki/Setting-up-a-development-environment).

### How can I support the team?

If you like what we do and would like to support us, you can donate through our [Patreon](https://www.patreon.com/nebula_mod_team). Thanks for the support <3
