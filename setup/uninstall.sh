#!/bin/bash
set -e

if [[ "$OSTYPE" != darwin* ]]; then
    >&2 echo "This script needs to run on macOS"
    exit 1
fi

rm -rf "/Applications/fuse X.app" \
       "/usr/local/lib/node_modules/@fuse-x" \
       "/usr/local/bin/fuse-x" \
       "/usr/local/bin/fuse" \
       "/usr/local/bin/uno"

echo "fuse X has been uninstalled"
