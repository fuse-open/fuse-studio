#!/bin/bash
set -e

MONO=mono
if [ "$OSTYPE" == "msys" ]; then
    MONO=""
fi

$MONO ./packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe --skipnontestassemblies $(find . -name '*Tests.dll' | grep '/bin/')

./regression-tests.sh
