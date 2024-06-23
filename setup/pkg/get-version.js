const fs = require('fs');
const data = fs.readFileSync(0, 'utf-8');

function isPositiveInteger(x) {
    // http://stackoverflow.com/a/1019526/11236
    return /^\d+$/.test(x);
}

function isVersionNumber(x) {
    const parts = x.split('.');
    for (var i = 0; i < parts.length; ++i) {
        if (!isPositiveInteger(parts[i])) {
            return false;
        }
    }
    return true;
}

const lines = data.split('\n');
const words = lines[0].split(' ');

for (let i = 0; i < words.length; i++) {
    let word = words[i].trim();

    if (word.startsWith("v"))
        word = word.substring(1);

    if (isVersionNumber(word)) {
        console.log(word);
        process.exit(0);
    }
}

process.exit(1);
