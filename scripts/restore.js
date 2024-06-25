const fs = require("fs");
const path = require("path");

function findup(suffix) {
    for (let dir = __dirname, parent = undefined;;
             dir = path.dirname(dir)) {

        if (dir == parent)
            throw Error(`${suffix} was not found`);

        parent = dir;
        const file = path.join(dir, suffix);

        if (fs.existsSync(file))
            return file;
    }
}

function findNodeModule(name) {
    const package = findup(`node_modules/${name}/package.json`);
    return path.dirname(package);
}

function restoreFiles(src, dsts) {
    for (file of fs.readdirSync(src)) {
        const srcf = path.join(src, file);

        for (dst2 of dsts) {
            const dstf = path.join(dst2, file);
            const placeholder = dstf + ".restore";

            if (!fs.existsSync(placeholder))
                continue;

            const relative = path.relative(process.cwd(), dstf);
            console.log(`restoring ${relative}`);
            fs.copyFileSync(srcf, dstf);
            fs.unlinkSync(placeholder);
        }
    }
}

const dst = path.join(__dirname, "..", "bin", "Release");
const dsts = [
    path.join(dst, "fuse X.app", "Contents", "MonoBundle"),
    path.join(dst, "fuse X (menu bar).app", "Contents", "MonoBundle"),
    path.join(dst, "UnoHost.app", "Contents", "MonoBundle"),
    dst
];

// Restore OpenTK (Windows only).
if (path.sep == "\\") {
    const opentk = findNodeModule("@fuse-open/opentk");
    restoreFiles(opentk, dsts);
}

// Restore Xamarin.Mac.
const xamarin = findNodeModule("@fuse-open/xamarin-mac");
restoreFiles(xamarin, dsts);

// Restore Uno binaries.
const uno = findNodeModule("@fuse-open/uno");
restoreFiles(path.join(uno, "bin"), dsts);

if (path.sep != "\\") {
    restoreFiles(path.join(dst, "fuse X.app", "Contents", "MonoBundle"), dsts);

    // Restore macOS icons.
    restoreFiles(path.join(dst, "fuse X.app", "Contents", "Resources"), [
        path.join(dst, "fuse X (menu bar).app", "Contents", "Resources")
    ]);
}

function patchConfig(file) {
    const config = fs.readFileSync(file).toString();
    if (config.indexOf("IsRoot: false") > 0)
        return;

    // Load parent config (fuse X).
    const relative = path.relative(process.cwd(), file);
    console.log(`patching ${relative}`);
    fs.appendFileSync(file, "\n// Load parent config");
    fs.appendFileSync(file, "\nIsRoot: false\n");
}

// Patch Uno config.
patchConfig(path.join(uno, ".unoconfig"));

function restoreGitignore(dir) {
    try {
        // Check .vscode subdirectory.
        const vscode = path.join(dir, ".vscode");
        if (fs.existsSync(vscode))
            restoreGitignore(vscode);

        // Restore .gitignore-files (renamed by 'npm pack').
        const gitignore = path.join(dir, ".gitignore");
        const npmignore = path.join(dir, ".npmignore");

        if (!fs.existsSync(gitignore) && 
                fs.existsSync(npmignore)) {
            console.log(`restoring ${gitignore}`);
            fs.renameSync(npmignore, gitignore);
        }
    } catch (e) {
        console.error(e);
    }
}

// FIXME: Programmatically enumerate all directories
//        under templates/projects/.
restoreGitignore("templates/projects/app");
restoreGitignore("templates/projects/example");
