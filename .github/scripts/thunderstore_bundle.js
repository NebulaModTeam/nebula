#!/usr/bin/env node

import {
  existsSync,
  mkdirSync,
  appendFileSync,
  readFileSync,
  writeFileSync,
  copyFileSync,
  readdirSync,
  statSync,
} from "fs";
import { join } from "path";
import fgrpkg from "@terascope/fetch-github-release";
const downloadRelease = fgrpkg;
import json2toml from "json2toml";
import child_process from "child_process";
import XmlReader from "xml-reader";
import xmlQuery from "xml-query";
import { zip } from 'zip-a-folder';
import fsepkg from 'fs-extra';
const move = fsepkg.move;
const remove = fsepkg.remove;
import * as core from '@actions/core';

// Setting it so that it's consistent with installs from thunderstore
const NEBULA_RELEASE_FOLDER_NAME = "nebula-NebulaMultiplayerMod";
const DIST_FOLDER = "dist";
const DIST_RELEASE_FOLDER = join(DIST_FOLDER, "release");
const DIST_NEBULA_FOLDER = join(DIST_RELEASE_FOLDER, "nebula");
const DIST_TSTORE_CLI_FOLDER = join(DIST_RELEASE_FOLDER, "tstore-cli");
const DIST_TSTORE_CLI_EXE_PATH = join(DIST_TSTORE_CLI_FOLDER, "tstore-cli.exe");
const DIST_TSTORE_CLI_CONFIG_PATH = join(
  DIST_TSTORE_CLI_FOLDER,
  "publish.toml"
);
const PLUGIN_INFO_PATH = "NebulaPatcher\\PluginInfo.cs";
const pluginInfo = getPluginInfo();
const TSTORE_ARCHIVE_PATH = join(DIST_RELEASE_FOLDER, "nebula-thunderstore.zip");
const GH_ARCHIVE_PATH = join(DIST_RELEASE_FOLDER, "Nebula_" + pluginInfo.version + ".zip");
const MOD_ICON_PATH = "thunderstore_icon.png";
const README_PATH = "README.md";
const CHANGELOG_PATH = "CHANGELOG.md";
const NEBULA_BINARIES_FOLDER = getNebulaFolder();

async function main() {
  if (!existsSync(DIST_NEBULA_FOLDER)) {
    mkdirSync(DIST_NEBULA_FOLDER, { recursive: true });
  }

  if (!existsSync(DIST_TSTORE_CLI_FOLDER)) {
    mkdirSync(DIST_TSTORE_CLI_FOLDER, { recursive: true });
  }

  try {
    generateReleaseBody();
  } catch(err) {
    core.setFailed(err);
  }
  
  generateManifest();
  copyIcon();
  copyReadme();
  appendChangelog();
  copyFolderContent(NEBULA_BINARIES_FOLDER, DIST_NEBULA_FOLDER);
  copyLicenses();

  await createTStoreArchive();
  await createGHArchive();

  downloadTStoreCli();
  generateTStoreConfig();
  uploadToTStore();
}

function getPluginInfo() {
  const pluginInfoRaw = readFileSync(PLUGIN_INFO_PATH).toString("utf-8");
  const versionInfoRaw = readFileSync("version.json").toString("utf-8");
  return {
    name: pluginInfoRaw.match(/PLUGIN_NAME = "(.*)";/)[1],
    id: pluginInfoRaw.match(/PLUGIN_ID = "(.*)";/)[1],
    version: JSON.parse(versionInfoRaw).version,
  };
}

function getNebulaFolder() {
  const targetsFile = "DevEnv.targets";

  var nebulaPath =
    "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Dyson Sphere Program";

  if (existsSync(targetsFile)) {
    const xml = XmlReader.parseSync(readFileSync(targetsFile, "utf-8"));
    const tmpPath = xmlQuery(xml).find("DSPGameDir").text();
    if (existsSync(tmpPath)) {
      nebulaPath = tmpPath;
    }
  }

  return join(nebulaPath, "BepInEx\\plugins\\Nebula");
}

function generateManifest() {
  const manifest = {
    name: pluginInfo.name,
    description:
      "With this mod you will be able to play with your friends in the same game!",
    version_number: pluginInfo.version,
    dependencies: ["xiaoye97-BepInEx-5.4.11", "PhantomGamers-IlLine-1.0.0"],
    website_url: "https://github.com/hubastard/nebula",
  };
  writeFileSync(
    join(DIST_NEBULA_FOLDER, "manifest.json"),
    JSON.stringify(manifest, null, 2)
  );
}

function copyIcon() {
  copyFileSync(MOD_ICON_PATH, join(DIST_NEBULA_FOLDER, "icon.png"));
}

function copyReadme() {
  copyFileSync(README_PATH, join(DIST_NEBULA_FOLDER, README_PATH));
}

function appendChangelog() {
  appendFileSync(
    join(DIST_NEBULA_FOLDER, README_PATH),
    "\n" + readFileSync(CHANGELOG_PATH)
  );
}

function copyLicenses() {
  copyFileSync("LICENSE", join(DIST_NEBULA_FOLDER, "nebula.LICENSE"));
  copyFolderContent("Licenses", DIST_NEBULA_FOLDER);
}

function copyFolderContent(src, dst, excludedExts) {
  readdirSync(src).forEach((file) => {
    const srcPath = join(src, file);
    const dstPath = join(dst, file);
    if (statSync(srcPath).isDirectory()) {
      if (!existsSync(dstPath)) {
        mkdirSync(dstPath);
        copyFolderContent(srcPath, dstPath, excludedExts);
      }
    } else {
      if (
        !excludedExts ||
        !excludedExts.includes(file.substr(file.lastIndexOf(".")))
      ) {
        copyFileSync(srcPath, dstPath);
      }
    }
  });
}

function downloadTStoreCli() {
  const user = "Windows10CE";
  const repo = "tstore-cli";
  const outputdir = DIST_TSTORE_CLI_FOLDER;
  const leaveZipped = false;
  const disableLogging = false;

  // Define a function to filter releases.
  function filterRelease(release) {
    // Filter out prereleases.
    return release.prerelease === false;
  }

  // Define a function to filter assets.
  function filterAsset(asset) {
    // Select assets that contain the string 'windows'.
    return asset.name.includes("tstore-cli.exe");
  }

  downloadRelease(
    user,
    repo,
    outputdir,
    filterRelease,
    filterAsset,
    leaveZipped,
    disableLogging
  )
    .then(function () {
      console.log("Successfully downloaded tstore-cli.exe");
    })
    .catch(function (err) {
      console.error(err.message);
    });
}

function generateTStoreConfig() {
  const config = {
    author: "nebula",
    communities: ["dyson-sphere-program"],
    nsfw: false,
    zip: TSTORE_ARCHIVE_PATH,
  };
  writeFileSync(DIST_TSTORE_CLI_CONFIG_PATH, json2toml(config));
}

function generateReleaseBody() {
  const changelog = readFileSync(CHANGELOG_PATH, "utf-8");
  const versionRegExp = new RegExp('\\b[0-9]+\\.[0-9]+(?:\\.[0-9]+)?(?:\\.[0-9]+)?(?=:)\\b', 'g');

  const versions = Array.from(changelog.matchAll(versionRegExp));

  const currentVersion = versions[0][0];

    if(pluginInfo.version != currentVersion)
    {
      throw `CHANGELOG.md latest version (${currentVersion}) does not match version.json (${pluginInfo.version}) !`;
    }

  const body = changelog.substr(versions[0].index + versions[0][0].length + 1, versions[1].index -  versions[0].index - versions[0][0].length - versions[1][0].length ).trim();

  writeFileSync(join(DIST_RELEASE_FOLDER, "BODY.md"), "# Alpha Version " + currentVersion + "\n\n### Changes\n" + body);

  console.log(body);
}

async function createTStoreArchive() {
  await zip(DIST_NEBULA_FOLDER, TSTORE_ARCHIVE_PATH);
}

async function createGHArchive() {
  // Ensure contents are within subfolder in zip
  await move(DIST_NEBULA_FOLDER, join(DIST_FOLDER, "tmp", NEBULA_RELEASE_FOLDER_NAME));
  await zip(join(DIST_FOLDER, "tmp"), GH_ARCHIVE_PATH);
  await move(join(DIST_FOLDER, "tmp", NEBULA_RELEASE_FOLDER_NAME), DIST_NEBULA_FOLDER);
  await remove(join(DIST_FOLDER, "tmp"));
}

function uploadToTStore() {
  child_process.execSync(
    DIST_TSTORE_CLI_EXE_PATH +
      " publish --config " +
      DIST_TSTORE_CLI_CONFIG_PATH,
      function(err) {
        console.error(err);
      }
  );
}

main();
