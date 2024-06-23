const path = require("path");
const {spawnSync} = require("child_process");

const NPM = path.sep == "\\" ? "npm.cmd" : "npm";
const PACKAGE = "android-build-tools"

function exec(cmd, argsArray) {
    const obj = spawnSync(cmd, argsArray, {stdio: 'inherit'});

    if (obj.error)
        throw obj.error;

    return obj.status;
}

function get_lines(cmd, argsArray) {
    const obj = spawnSync(cmd, argsArray);

    if (obj.error)
        throw obj.error;

    const stdout = String(obj.stdout);
    return stdout.split(/\r?\n/);
}

function install(name) {
    return exec(NPM, ["install", "-g", "-f", name]);
}

function list() {
    return get_lines(NPM, ["list", "-g", "--depth", "0"]);
}

for (let arg of process.argv.splice(2)) {
    switch (arg) {
    case '-s':
    case '--status':
        for (item of list()) {
            if (item.includes(PACKAGE)) {
                console.log("Up to date.");
                process.exit(0);
            }
        }

        process.exit(100);
    }
}

install(PACKAGE);
