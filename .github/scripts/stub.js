#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync } from "fs";
import { basename, dirname, extname, join, resolve } from "path";
import child_process from "child_process";

const DSP_DIR =
  "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Dyson Sphere Program\\";
const BEPINEX_DIR = join(DSP_DIR, "BepInEx\\core\\");
const DSP_ASSEMBLY_DIR = join(DSP_DIR, "DSPGAME_Data\\Managed\\");

const refFile = readReferenceFile();

function main() {
  if (!existsSync("Libs")) {
    mkdirSync("Libs");
  }
  publicizeReferenceAssemblies(getReferencePaths());
  stubAssemblies(getReferencePaths());
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

function publicizeReferenceAssemblies(refPaths) {
  const ASSEMBLYPUBLICIZER_PATH = resolve(
    "Libs\\AssemblyPublicizer\\AssemblyPublicizer.exe"
  );
  Array.from(refPaths).forEach((line) => {
    console.log("Publicizing " + line);
    child_process.execSync(
      'cd "' +
        dirname(line) +
        '" && "' +
        ASSEMBLYPUBLICIZER_PATH +
        '" "' +
        line +
        '"'
    );
  });
}

function stubAssemblies(refPaths) {
  const REFASMER_PATH =
    '"' + resolve("Libs\\Refasmer.net461\\RefasmerExe.exe") + '"';
  const LIBS_PATH = '"' + resolve("Libs\\") + '"';
  Array.from(refPaths).forEach((line) => {
    console.log("Stubbing " + line);
    const PUBLICIZED_ASSEMBLIES_PATH =
      '"' + dirname(line) + '\\publicized_assemblies\\"';
    const PUBLICIZED_ASSEMBLY_PATH =
      '"' +
      dirname(line) +
      "\\publicized_assemblies\\" +
      basename(line);
      // needed for original version of assembly publicizer
      //basename(line).slice(0, basename(line).length - extname(line).length) +
      //"_publicized" +
      //extname(line);
    const cmd =
      "cd " +
      PUBLICIZED_ASSEMBLIES_PATH +
      " && " +
      REFASMER_PATH +
      " " +
      PUBLICIZED_ASSEMBLY_PATH +
      '" && move ' +
      PUBLICIZED_ASSEMBLY_PATH +
      '.refasm.dll" ' +
      LIBS_PATH;
    child_process.execSync(cmd);
  });
}

main();
