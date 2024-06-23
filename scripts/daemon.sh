#!/bin/bash
#
# Launch the daemon based on a copy of fuse X assemblies (Windows).
#
# This avoids write-locking the assemblies and thus allowing us to work on
# fuse X while the daemon is running.
#
set -e
SRC="bin/Debug"
DST="$SRC-copy"

# Kill running daemons first, if any.
bash scripts/kill.sh

if [ "$OSTYPE" = msys ]; then
    mkdir -p "$DST"
    cp -fu "$SRC"/*.{dll,exe} "$DST"
    $DST/fuse daemon "$@"
else
    # No need to copy on other platforms.
    node $SRC/fuse.js daemon "$@"
fi
