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
import child_process from "child_process";
import { zip } from "zip-a-folder";
import fsepkg, { copy } from "fs-extra";
const remove = fsepkg.remove;
import * as core from "@actions/core";

// Setting it so that it's consistent with installs from thunderstore
const NEBULA_RELEASE_FOLDER_NAME = "nebula-NebulaMultiplayerMod";
const NEBULA_API_RELEASE_FOLDER_NAME = "nebula-NebulaMultiplayerModApi";
const DIST_FOLDER = "dist";
const DIST_RELEASE_FOLDER = join(DIST_FOLDER, "release");
const DIST_NEBULA_FOLDER = join(
  DIST_RELEASE_FOLDER,
  NEBULA_RELEASE_FOLDER_NAME
);
const DIST_NEBULA_API_FOLDER = join(
  DIST_RELEASE_FOLDER,
  NEBULA_API_RELEASE_FOLDER_NAME
);
const DIST_TSTORE_CLI_FOLDER = join("Libs", "tcli");
const DIST_TSTORE_CLI_EXE_PATH = join(DIST_TSTORE_CLI_FOLDER, "tcli.exe");
const PLUGIN_INFO_PATH = "NebulaPatcher\\PluginInfo.cs";
const API_PLUGIN_INFO_PATH = "NebulaAPI\\NebulaModAPI.cs";
const pluginInfo = getPluginInfo();
const apiPluginInfo = getApiPluginInfo();
const TSTORE_ARCHIVE_PATH = join(
  DIST_RELEASE_FOLDER,
  "nebula-thunderstore.zip"
);
const TSTORE_API_ARCHIVE_PATH = join(
  DIST_RELEASE_FOLDER,
  "nebula-api-thunderstore.zip"
);
const GH_ARCHIVE_PATH = join(
  DIST_RELEASE_FOLDER,
  "Nebula_" + pluginInfo.version + ".zip"
);
const MOD_ICON_PATH = "thunderstore_icon.png";
const README_PATH = "README.md";
const CHANGELOG_PATH = "CHANGELOG.md";
const BEPINEX_DEPENDENCY = "xiaoye97-BepInEx-5.4.17";

async function main() {
  if (!existsSync(DIST_NEBULA_FOLDER)) {
    let err = DIST_NEBULA_FOLDER + " does not exist";
    core.setFailed(err);
    throw err;
  }

  if (!existsSync(DIST_NEBULA_API_FOLDER)) {
    let err = DIST_NEBUDIST_NEBULA_API_FOLDERLA_FOLDER + " does not exist";
    core.setFailed(err);
    throw err;
  }

  try {
    generateReleaseBody();
  } catch (err) {
    core.setFailed(err);
    throw err;
  }

  generateManifest();
  generateApiManifest();
  copyIcon();
  copyApiIcon();
  copyReadme();
  copyApiReadme();
  copyChangelog();
  appendApiChangelog();
  copyLicenses();
  copyApiLicense();

  await createTStoreArchive();
  await createTStoreApiArchive();
  await createGHArchive();

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

function getApiPluginInfo() {
  const pluginInfoRaw = readFileSync(API_PLUGIN_INFO_PATH).toString("utf-8");
  const versionInfoRaw = readFileSync(
    join("NebulaAPI", "version.json")
  ).toString("utf-8");
  return {
    name: pluginInfoRaw.match(/API_NAME = "(.*)";/)[1],
    id: pluginInfoRaw.match(/API_GUID = "(.*)";/)[1],
    version: JSON.parse(versionInfoRaw).version,
  };
}

function generateManifest() {
  const manifest = {
    name: pluginInfo.name,
    description:
      "With this mod you will be able to play with your friends in the same game! Now supports combat mode in game version 0.10.32",
    version_number: pluginInfo.version,
    dependencies: [
      BEPINEX_DEPENDENCY,
      `nebula-${apiPluginInfo.name}-${apiPluginInfo.version}`,
      "PhantomGamers-IlLine-1.0.0",
      "starfi5h-BulletTime-1.5.5",
    ],
    website_url: "https://github.com/hubastard/nebula"
  };
  writeFileSync(
    join(DIST_NEBULA_FOLDER, "manifest.json"),
    JSON.stringify(manifest, null, 2)
  );
}

function generateApiManifest() {
  const manifest = {
    name: apiPluginInfo.name,
    description: "API for other mods to work with the Nebula Multiplayer Mod. (Does NOT require Nebula)",
    version_number: apiPluginInfo.version,
    dependencies: [
      BEPINEX_DEPENDENCY
    ],
    website_url: "https://github.com/hubastard/nebula"
  };
  writeFileSync(
    join(DIST_NEBULA_API_FOLDER, "manifest.json"),
    JSON.stringify(manifest, null, 2)
  );
}

function copyIcon() {
  copyFileSync(MOD_ICON_PATH, join(DIST_NEBULA_FOLDER, "icon.png"));
}

function copyApiIcon() {
  copyFileSync(
    join("NebulaAPI", "icon.png"),
    join(DIST_NEBULA_API_FOLDER, "icon.png")
  );
}

function copyReadme() {
  copyFileSync(README_PATH, join(DIST_NEBULA_FOLDER, README_PATH));
}

function copyApiReadme() {
  copyFileSync(
    join("NebulaAPI", README_PATH),
    join(DIST_NEBULA_API_FOLDER, README_PATH)
  );
}

function copyChangelog() {
  copyFileSync(CHANGELOG_PATH, join(DIST_NEBULA_FOLDER, CHANGELOG_PATH));
}

function appendApiChangelog() {
  appendFileSync(
    join(DIST_NEBULA_API_FOLDER, README_PATH),
    "\n" + readFileSync(join("NebulaAPI", CHANGELOG_PATH))
  );
}

function copyLicenses() {
  copyFileSync("LICENSE", join(DIST_NEBULA_FOLDER, "nebula.LICENSE"));
  copyFolderContent("Licenses", DIST_NEBULA_FOLDER);
}

function copyApiLicense() {
  copyFileSync("LICENSE", join(DIST_NEBULA_API_FOLDER, "nebula.LICENSE"));
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

function generateReleaseBody() {
  const changelog = readFileSync(CHANGELOG_PATH, "utf-8");
  const versionRegExp = new RegExp(
    "\\b[0-9]+\\.[0-9]+(?:\\.[0-9]+)?(?:\\.[0-9]+)?(?=:)\\b",
    "g"
  );

  const versions = Array.from(changelog.matchAll(versionRegExp));

  const currentVersion = versions[0][0];

  if (pluginInfo.version != currentVersion) {
    throw `CHANGELOG.md latest version (${currentVersion}) does not match version.json (${pluginInfo.version}) !`;
  }

  const body = changelog
    .substr(
      versions[0].index + versions[0][0].length + 1,
      versions[1].index -
        versions[0].index -
        versions[0][0].length -
        versions[1][0].length
    )
    .trim();

  writeFileSync(
    join(DIST_RELEASE_FOLDER, "BODY.md"),
    "# Alpha Version " + currentVersion + "\n\n### Changes\n" + body
  );
}

async function createTStoreArchive() {
  await zip(DIST_NEBULA_FOLDER, TSTORE_ARCHIVE_PATH);
}

async function createTStoreApiArchive() {
  await zip(DIST_NEBULA_API_FOLDER, TSTORE_API_ARCHIVE_PATH);
}

async function createGHArchive() {
  // Ensure contents are within subfolder in zip
  await copy(
    DIST_NEBULA_FOLDER,
    join(DIST_FOLDER, "tmp", NEBULA_RELEASE_FOLDER_NAME)
  );
  await copy(
    DIST_NEBULA_API_FOLDER,
    join(DIST_FOLDER, "tmp", NEBULA_API_RELEASE_FOLDER_NAME)
  );
  await zip(join(DIST_FOLDER, "tmp"), GH_ARCHIVE_PATH);
  await remove(join(DIST_FOLDER, "tmp"));
}

function uploadToTStore() {
  try {
    child_process.execSync(
      `"${DIST_TSTORE_CLI_EXE_PATH}" publish --file "${TSTORE_API_ARCHIVE_PATH}"`
    );
  } catch (error) {
    console.error(`Thunderstore upload failed for ${TSTORE_API_ARCHIVE_PATH} with error ${error}`);
  }

  try {
    child_process.execSync(
      `"${DIST_TSTORE_CLI_EXE_PATH}" publish --file "${TSTORE_ARCHIVE_PATH}"`
    );
  } catch (error) {
    console.error(`Thunderstore upload failed for ${TSTORE_ARCHIVE_PATH} with error ${error}`);
  }
}

main();
