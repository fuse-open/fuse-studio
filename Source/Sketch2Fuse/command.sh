#!/bin/bash
set -e

MONO=mono
if [ "$OSTYPE" == "msys" ]; then
    MONO=""
fi

$MONO Command/bin/Debug/Command.exe $*
