const fs = require('fs');
const path = require('path');
const os = require('os');

// Copy generated .unoconfig to %PROGRAMDATA% (#92).
function copyUnoConfig() {

    // We've seen %PROGRAMDATA% being empty on some systems.
    const dir = process.env.PROGRAMDATA && process.env.PROGRAMDATA.length
            ? path.join(process.env.PROGRAMDATA, 'fuse X')
            : path.join(process.env.SYSTEMDRIVE, 'ProgramData', 'fuse X')

    const src = path.join(os.homedir(), '.unoconfig');
    const dst = path.join(dir, '.unoconfig');

    if (fs.existsSync(src)) {
        if (!fs.existsSync(dir))
            fs.mkdirSync(dir, {recursive: true});

        fs.copyFileSync(src, dst);
        console.log(dst);
    }
}

try {
    copyUnoConfig();
} catch (e) {
    console.error(e);
    process.exit(1);
}
