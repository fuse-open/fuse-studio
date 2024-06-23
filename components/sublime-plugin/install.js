const path = require("path");
const fs = require('fs');

const PACKAGE = "plugin.zip";

const appdata_dir = path.sep == '\\'
                    ? process.env.APPDATA
                    : path.join(process.env.HOME, "Library", "Application Support");
const sublime_data_dir = path.join(appdata_dir, "Sublime Text 3");
const sublime_package_dir = path.join(sublime_data_dir, "Installed Packages");
const fuse_package_file = path.join(sublime_package_dir, "Fuse.sublime-package");

for (let arg of process.argv.splice(2)) {
    switch (arg) {
    case '-s':
    case '--status':
        if (fs.existsSync(fuse_package_file)) {
            console.log("Up to date.");
            process.exit(0);
        } else {
            process.exit(100);
        }
    }
}

if (!fs.existsSync(sublime_package_dir))
    fs.mkdirSync(sublime_package_dir, {recursive: true});

fs.copyFileSync(path.join(__dirname, PACKAGE), fuse_package_file);
console.log("Install completed.");
