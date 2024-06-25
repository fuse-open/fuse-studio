#!/usr/bin/env node
const path = require('path');
const {spawn} = require('child_process');

spawn(path.join(__dirname,
            path.sep == '\\'
                ? "fuse.exe"
                : "fuse"),
        process.argv.splice(2),
        {stdio: 'inherit'})
    .on('exit', process.exit);
