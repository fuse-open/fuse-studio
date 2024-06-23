#!/bin/bash
set -e

# Mono and NPM
export PATH=/usr/local/bin:/opt/local/bin:/opt/homebrew/bin:$PATH
export PATH=/Library/Frameworks/Mono.framework/Versions/Current/Commands:$PATH

# Config
TGZ=/tmp/fuse-x-studio-mac.tar
PREFIX=/usr/local
DST=$PREFIX/lib/node_modules/@fuse-x/studio-mac
FUSE=$PREFIX/bin/fuse
UNO=$PREFIX/bin/uno

# Override tarball
if [ -n "$1" ]; then
    TGZ=$1
fi

# Debug
echo USER:  $USER
echo HOME:  $HOME
echo TGZ:   $TGZ
echo DST:   $DST
echo FUSE:  $FUSE
echo UNO:   $UNO

# Install
npm install -g -f dotnet-run --prefix "$PREFIX"
npm install -g -f "$TGZ" --prefix "$PREFIX"
"$FUSE" kill-all

# Warm-up
"$UNO" build "$DST/empty"
