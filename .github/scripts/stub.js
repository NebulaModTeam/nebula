#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync } from "fs";
import { basename, dirname, join, resolve } from "path";
import child_process from "child_process";
import XmlReader from "xml-reader";
import xmlQuery from "xml-query";

const DSP_DIR = getDSPFolder();
const BEPINEX_DIR = join(DSP_DIR, "BepInEx\\core\\");
const DSP_ASSEMBLY_DIR = join(DSP_DIR, "DSPGAME_Data\\Managed\\");

const refFile = readReferenceFile();

function main() {
  if (!existsSync("Libs")) {
    mkdirSync("Libs");
  }
  publicizeAndStubAssemblies(getReferencePaths());
}

function getDSPFolder() {
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

  return join(nebulaPath);
}

function readReferenceFile() {
  const refFile = readFileSync("references.txt", "utf8");

  if (!refFile) {
    throw "Could not read references.txt";
  }

  return refFile;
}

function getReferencePaths() {
  return refFile
    .replaceAll("$(BepInExDir)", BEPINEX_DIR)
    .replaceAll("$(DSPAssemblyDir)", DSP_ASSEMBLY_DIR)
    .trim()
    .split("\n");
}

function publicizeAndStubAssemblies(refPaths) {
  const ASSEMBLYPUBLICIZER_PATH = resolve(
    "Libs\\AssemblyPublicizer\\AssemblyPublicizer.exe"
  );
  const LIBS_PATH = resolve("Libs");
  Array.from(refPaths).forEach((line) => {
    console.log("Publicizing and stubbing " + line);
    child_process.execSync(
      'cd "' +
        dirname(line) +
        '" && "' +
        ASSEMBLYPUBLICIZER_PATH +
        '" "' +
        line +
        '" && copy "' +
        join(dirname(line), "publicized_assemblies", basename(line)) +
        '" "' +
        LIBS_PATH +
        '"'
    );
  });
}

main();
