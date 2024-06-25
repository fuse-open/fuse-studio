const path = require("path");
const {spawnSync} = require("child_process");

const CODE = path.sep == "\\" ? "code.cmd" : "code";
const EXTENSION = "fuseopen.fuse-vscode";

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

    const stderr = String(obj.stderr);
    const stdout = String(obj.stdout);

    if (stderr && stderr.length)
        console.error(stderr);

    return stdout.split(/\r?\n/);
}

function get_extensions() {
    return get_lines(CODE, ["--list-extensions"]);
}

function install(name) {
    exec(CODE, ["--install-extension", name, "--force"]);
}

function uninstall(name) {
    exec(CODE, ["--uninstall-extension", name]);
}

for (let arg of process.argv.splice(2)) {
    switch (arg) {
    case '-s':
    case '--status':
        for (let name of get_extensions()) {
            if (name == EXTENSION) {
                console.log("Up to date.");
                process.exit(0);
            }
        }

        process.exit(100);
    }
}

// Uninstall legacy extensions.
for (let name of get_extensions()) {
    switch (name) {
    case 'iGN97.fuse-vscode':
    case 'naumovs.vscode-fuse-syntax':
        try {
            uninstall(name);
        } catch (e) {
            console.error("W: " + e);
        }
    }
}

install(EXTENSION);
