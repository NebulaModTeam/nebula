#!/usr/bin/env node

import {
  existsSync,
  mkdirSync,
  appendFileSync,
  readFileSync,
  writeFileSync,
  copyFileSync,
  createWriteStream,
  readdirSync,
  statSync,
} from "fs";
import { join } from "path";
import pkg from "@terascope/fetch-github-release";
const downloadRelease = pkg;
import json2toml from "json2toml";
import JSZip from "jszip";
import child_process from "child_process";
import XmlReader from "xml-reader";
import xmlQuery from "xml-query";

const DIST_FOLDER = "dist";
const DIST_TSTORE_FOLDER = join(DIST_FOLDER, "thunderstore");
const DIST_NEBULA_FOLDER = join(DIST_TSTORE_FOLDER, "nebula");
const DIST_TSTORE_CLI_FOLDER = join(DIST_TSTORE_FOLDER, "tstore-cli");
const DIST_TSTORE_CLI_EXE_PATH = join(DIST_TSTORE_CLI_FOLDER, "tstore-cli.exe");
const DIST_TSTORE_CLI_CONFIG_PATH = join(
  DIST_TSTORE_CLI_FOLDER,
  "publish.toml"
);
const ARCHIVE_PATH = join(DIST_TSTORE_FOLDER, "nebula.zip");
const PLUGIN_INFO_PATH = "NebulaPatcher\\PluginInfo.cs";
const MOD_ICON_PATH = "thunderstore_icon.png";
const README_PATH = "README.md";
const CHANGELOG_PATH = "CHANGELOG.md";
const NEBULA_BINARIES_FOLDER = getNebulaFolder();

function main() {
  if (!existsSync(DIST_NEBULA_FOLDER)) {
    mkdirSync(DIST_NEBULA_FOLDER, { recursive: true });
  }

  if (!existsSync(DIST_TSTORE_CLI_FOLDER)) {
    mkdirSync(DIST_TSTORE_CLI_FOLDER, { recursive: true });
  }

  generateManifest();
  copyIcon();
  copyReadme();
  appendChangelog();
  copyFolderContent(NEBULA_BINARIES_FOLDER, DIST_NEBULA_FOLDER, [".pdb"]);
  copyLicenses();

  archiveNebula();

  downloadTStoreCli();
  generateTStoreConfig();
  uploadToTStore();
}

function getPluginInfo() {
  const pluginInfoRaw = readFileSync(PLUGIN_INFO_PATH).toString("utf-8");
  return {
    name: pluginInfoRaw.match(/PLUGIN_NAME = "(.*)";/)[1],
    id: pluginInfoRaw.match(/PLUGIN_ID = "(.*)";/)[1],
    version: pluginInfoRaw.match(/PLUGIN_VERSION = "(.*)";/)[1],
  };
}

function getNebulaFolder() {
  const targetsFile = "DevEnv.targets";

  var nebulaPath =
    "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Dyson Sphere Program\\BepInEx\\plugins\\Nebula";

  if (existsSync(targetsFile)) {
    const xml = XmlReader.parseSync(readFileSync(targetsFile, "utf-8"));
    var tmpPath = xmlQuery(xml).find("DSPGameDir").text();
    if (existsSync(tmpPath)) {
      nebulaPath = tmpPath;
    }
  }

  return nebulaPath;
}

function generateManifest() {
  const pluginInfo = getPluginInfo();
  const manifest = {
    name: pluginInfo.name,
    description:
      "With this mod you will be able to play with your friends in the same game!",
    version_number: pluginInfo.version,
    dependencies: ["xiaoye97-BepInEx-5.4.11"],
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
    zip: ARCHIVE_PATH,
  };
  writeFileSync(DIST_TSTORE_CLI_CONFIG_PATH, json2toml(config));
}

function archiveNebula() {
  var zip = new JSZip();
  zip.folder(DIST_NEBULA_FOLDER);
  zip
    .generateNodeStream({ type: "nodebuffer", streamFiles: true })
    .pipe(createWriteStream(ARCHIVE_PATH))
    .on("finish", function () {
      console.log("Created nebula archive");
    });
}

function uploadToTStore() {
  child_process.execSync(
    DIST_TSTORE_CLI_EXE_PATH +
      " publish --config " +
      DIST_TSTORE_CLI_CONFIG_PATH
  );
}

main();
