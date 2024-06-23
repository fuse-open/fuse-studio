#!/bin/bash
set -e

MONO=mono
if [ "$OSTYPE" == "msys" ]; then
    MONO=""
fi

$MONO RegressionTests/bin/Debug/RegressionTests.exe $*
