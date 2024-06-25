const fs = require("fs");
const path = require("path");

const filename = process.argv[2]
const platform = process.argv[3]

if (!filename || !platform)
    throw new Error("Please provide filename and platform arguments.")

console.log("updating " + filename + " (" + platform + ")")

const text = fs.readFileSync(filename, "utf8");
const json = JSON.parse(text);

json.name = json.name + "-" + platform
json.bin = json["bin-" + platform]
json.os = json["os-" + platform]
json.scripts = json["scripts-" + platform]

delete json["bin-mac"]
delete json["bin-win"]
delete json["os-mac"]
delete json["os-win"]
delete json["scripts-mac"]
delete json["scripts-win"]
delete json.devDependencies

fs.writeFileSync(filename, JSON.stringify(json, undefined, 2));
