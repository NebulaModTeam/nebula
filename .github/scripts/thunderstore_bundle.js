#!/usr/bin/env node

const fs = require('fs')
const path = require('path')

const DIST_FOLDER = 'dist\\thunderstore\\nebula';
const PLUGIN_INFO = 'NebulaPatcher\\PluginInfo.cs'
const MOD_ICON = 'thunderstore_icon.png';
const README = 'README.md';
const NEBULA_BINARIES = 'C:\\Program Files (x86)\\Steam\\steamapps\\common\\Dyson Sphere Program\\BepInEx\\plugins\\Nebula';

function main() {
    if (!fs.existsSync(DIST_FOLDER)) {
        fs.mkdirSync(DIST_FOLDER, {recursive: true})
    }

    generateManifest()
    copyIcon()
    copyReadme()
    copyFolderContent(NEBULA_BINARIES, DIST_FOLDER, ['.pdb'])
    copyLicenses()
}

function getPluginInfo() {
    const pluginInfoRaw = fs.readFileSync(PLUGIN_INFO).toString("utf-8")
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
    fs.writeFileSync(path.join(DIST_FOLDER, 'manifest.json'), JSON.stringify(manifest, null, 2))
}

function copyIcon() {
    fs.copyFileSync(MOD_ICON, path.join(DIST_FOLDER, 'icon.png'))
}

function copyReadme() {
    fs.copyFileSync(README, path.join(DIST_FOLDER, 'README.md'))
}

function copyLicenses() {
    fs.copyFileSync('LICENSE', path.join(DIST_FOLDER, 'nebula.LICENSE'))
    copyFolderContent('Licenses', DIST_FOLDER)
}

function copyFolderContent(src, dst, excludedExts) {
    fs.readdirSync(src).forEach(file => {
        const srcPath = path.join(src, file)
        const dstPath = path.join(dst, file)
        if (fs.statSync(srcPath).isDirectory() ) {
            if (!fs.existsSync(dstPath)) {
                fs.mkdirSync(dstPath)
                copyFolderContent(srcPath, dstPath, excludedExts)
            }
        } else {
            if (!excludedExts || !excludedExts.includes(file.substr(file.lastIndexOf('.')))) {
                fs.copyFileSync(srcPath, dstPath)
            }
        }
    })
}

main();
