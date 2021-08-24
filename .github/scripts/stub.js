#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync } from "fs";
import { basename, join, resolve } from "path";
import child_process from "child_process";
import XmlReader from "xml-reader";
import xmlQuery from "xml-query";

const DSP_DIR = getDSPFolder();
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
  const refFile = readFileSync("Directory.Build.props", "utf8");

  if (!refFile) {
    throw "Could not read Directory.Build.props";
  }

  return refFile;
}

function getReferencePaths() {
  return refFile
    .match(/\$\(PropsheetPath\)(.*)(?=\" P)/g)
    .join("\n")
    .replaceAll("$(PropsheetPath)\\Libs", DSP_ASSEMBLY_DIR)
    .trim()
    .split("\n");
}

function publicizeAndStubAssemblies(refPaths) {
  const NSTRIP_PATH = resolve("Libs\\NStrip\\NStrip.exe");
  const LIBS_PATH = resolve("Libs");
  Array.from(refPaths).forEach((line) => {
    console.log("Publicizing and stubbing " + line);
    child_process.execSync(
      `"${NSTRIP_PATH}" -p -cg --cg-exclude-events "${line}" "${join(LIBS_PATH, basename(line))}"`
    );
  });
}

main();
