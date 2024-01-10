# Nebula Multiplayer Mod [![GitHub Release](https://img.shields.io/github/v/release/NebulaModTeam/nebula)](https://github.com/NebulaModTeam/nebula/releases/latest) [![Nightly Build](https://img.shields.io/badge/nightly-Build-8A2BE2?link=https%3A%2F%2Fnightly.link%2FNebulaModTeam%2Fnebula%2Fworkflows%2Fbuild-winx64%2Fmaster%2Fbuild-artifacts-Release.zip)](https://nightly.link/NebulaModTeam/nebula/workflows/build-winx64/master/build-artifacts-Release.zip) [![Build - Win x64](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml/badge.svg)](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml)

An open-source, multiplayer mod for the game [Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).

# Download Prerelease

[![Build - Win x64](https://github.com/NebulaModTeam/nebula/actions/workflows/build-winx64.yml/badge.svg?branch=master)](https://nightly.link/NebulaModTeam/nebula/workflows/build-winx64/master/build-artifacts-Release.zip)
Note: This is the bleeding edge build, for the more stable build see below. You can use this one if you want to try out the latest development builds, which may contain bugs and unfinished work.

You will also need some extra steps to get this installed, see 2nd point in ["How can I play this mod?"](#how-can-i-play-this-mod).

# FAQ

## Where can I get mod support?

Please join our [Discord Server](https://discord.gg/UHeB2QvgDa) and ask your question in the `General` channel. We have a really nice community that will be able to answer your questions.

## How can I play this mod?

Please do keep in mind that this mod is still in heavy development, it may still contains bugs.

- Stable version of the mod can be downloaded from [Thunderstore](https://dsp.thunderstore.io/package/nebula/NebulaMultiplayerMod/) (Recommended).
- If you want to install the latest version of the mod, you can install pre-release versions be following the [installation guide](https://github.com/hubastard/nebula/wiki/Installing-a-pre-release-version).
- To connect, check [hosting and joining guide](https://github.com/hubastard/nebula/wiki/Hosting-and-Joining).

## API Documentation

This mod has an API, that makes it easier for other mod developers to make their mods compatible with Nebula. If you are a mod developer and you want your mods to be compatible, follow the instructions [here](https://github.com/hubastard/nebula/wiki/Nebula-mod-API). Also you can always join our [Discord Server](https://discord.gg/UHeB2QvgDa) for help with using the API.

## Chat 

The chat window can opened/closed using Alt + Backtick (configurable in game settings under Control). Also in settings (under Multiplayer) is an option to disable the chat window from automatically opening when a message is received.

## What is the current status?

Major refactors will happen while the project grows. Join the [Discord Server](https://discord.gg/UHeB2QvgDa) if you want to see to latest state of our development.

The prerelease version does support DSP `0.10.x`, but dark fog enemies and buildings are not supported as of this date (09. January 2024). This is what we will focus on now.

<details>
<summary>Here is a short list of what is currently implemented (outdated, we support more)</summary>

- [x] Server / Client communication
- [x] Custom Multiplayer menu in-game
- [x] Player Movement syncing on Planet
- [x] Player Movement syncing in Space
- [x] Player VFX syncing (jetpack, torch, ...)
- [x] Player SFX syncing (footsteps sound, torch sound, ...)
- [x] Players have different colors
- [x] Game Time syncing
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
- [x] Factories statistics syncing
- [x] Containers inventory syncing
- [x] Building Interaction syncing
- [x] Belts syncing
- [x] Dropped items syncing
- [x] Interstellar Station syncing
- [x] Drones position syncing
- [x] Foundation syncing (terrain deformation)
- [x] Server state persistence
- [x] Power network syncing

</details>

## How can I contribute?

Please join our [Discord Server](https://discord.gg/UHeB2QvgDa) to ask if someone is already working on the task that you want to do. Once, you are done with your modification, simply submit a pull request. Contribution documentation can be found here: [Wiki](https://github.com/hubastard/nebula/wiki/Setting-up-a-development-environment).

## How can I support the team?

If you like what we do and would like to support us, you can donate through our [Patreon](https://www.patreon.com/nebula_mod_team). Thanks for the support <3

