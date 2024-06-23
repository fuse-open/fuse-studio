#!/bin/bash

function error {
    echo "Report issues at https://github.com/fuse-x/studio/issues"
    echo "ALERT:Something went wrong|See 'Details' for more information or try reinstalling the app."
    exit 1
}

APP="/usr/local/lib/node_modules/@fuse-x/studio-mac/bin/Release/fuse X.app"
PROC=$(ps aux | grep "$APP" | grep -v "grep")

open "$APP" || error

if [ -z "$PROC" ]; then
    echo "Waiting for fuse X to open"
    sleep 4
fi

if [ -n "$1" ]; then
    echo "Running uri-handler"
    /usr/local/bin/fuse uri "$1" || error
fi

echo "QUITAPP"
