#!/usr/bin/env node

import { existsSync, mkdirSync, appendFileSync, readFileSync, writeFileSync, copyFileSync, readdirSync, statSync } from 'fs';
import { join } from 'path';

const DIST_FOLDER = 'dist\\thunderstore\\nebula';
const PLUGIN_INFO = 'NebulaPatcher\\PluginInfo.cs'
const MOD_ICON = 'thunderstore_icon.png';
const README = 'README.md';
const CHANGELOG = 'CHANGELOG.md';
const NEBULA_BINARIES = 'C:\\Program Files (x86)\\Steam\\steamapps\\common\\Dyson Sphere Program\\BepInEx\\plugins\\Nebula';

function main() {
    if (!existsSync(DIST_FOLDER)) {
        mkdirSync(DIST_FOLDER, {recursive: true})
    }

    generateManifest()
    copyIcon()
    copyReadme()
    appendChangelog()
    copyFolderContent(NEBULA_BINARIES, DIST_FOLDER, ['.pdb'])
    copyLicenses()
}

function getPluginInfo() {
    const pluginInfoRaw = readFileSync(PLUGIN_INFO).toString("utf-8")
    return {
        name: pluginInfoRaw.match(/PLUGIN_NAME = "(.*)";/)[1],
        id: pluginInfoRaw.match(/PLUGIN_ID = "(.*)";/)[1],
        version: pluginInfoRaw.match(/PLUGIN_VERSION = "(.*)";/)[1],
    }
}

function generateManifest() {
    const pluginInfo = getPluginInfo();
    const manifest = {
        name: pluginInfo.name,
        description: "With this mod you will be able to play with your friends in the same game!",
        version_number: pluginInfo.version,
        dependencies: ["xiaoye97-BepInEx-5.4.11"],
        website_url: "https://github.com/hubastard/nebula"
    }
    writeFileSync(join(DIST_FOLDER, 'manifest.json'), JSON.stringify(manifest, null, 2))
}

function copyIcon() {
    copyFileSync(MOD_ICON, join(DIST_FOLDER, 'icon.png'))
}

function copyReadme() {
    copyFileSync(README, join(DIST_FOLDER, README))
}

function appendChangelog() {
    appendFileSync(join(DIST_FOLDER, README), "\n" + readFileSync(CHANGELOG))
}

function copyLicenses() {
    copyFileSync('LICENSE', join(DIST_FOLDER, 'nebula.LICENSE'))
    copyFolderContent('Licenses', DIST_FOLDER)
}

function copyFolderContent(src, dst, excludedExts) {
    readdirSync(src).forEach(file => {
        const srcPath = join(src, file)
        const dstPath = join(dst, file)
        if (statSync(srcPath).isDirectory() ) {
            if (!existsSync(dstPath)) {
                mkdirSync(dstPath)
                copyFolderContent(srcPath, dstPath, excludedExts)
            }
        } else {
            if (!excludedExts || !excludedExts.includes(file.substr(file.lastIndexOf('.')))) {
                copyFileSync(srcPath, dstPath)
            }
        }
    })
}

main();