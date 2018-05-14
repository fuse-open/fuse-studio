#!/bin/sh
set -e
cd "`dirname "$0"`"

if [ "$OSTYPE" = msys ]; then
    exe_prefix=""
else
    exe_prefix="mono"
fi

if [ -z "$CONFIG" ]; then
    CONFIG="Debug"
fi

help()
{
    echo "This script executes Fuse tests locally. It doesn't automatically build Fuse, so remember to run 'build.sh' first!"    
    echo "By default it runs all the tests, however a filter can be specified by providing '--where'. See https://github.com/nunit/docs/wiki/Test-Selection-Language for this syntax."
    echo 
    echo "All arguments are forwarded to NUnit3 Console, so see https://github.com/nunit/docs/wiki/Console-Command-Line for more help."
    exit
}

if [ "$1" = "-h" ] || [ "$1" = "--help" ]; then
    help
fi

NUNIT_VERSION=3.6.0

# Install NUnit.Console
$exe_prefix .nuget/nuget.exe install NUnit.Console -version $NUNIT_VERSION -o packages

# The tests may create temporary files that we don't want git to find
script_path=`pwd`
mkdir -p bin/Tests
pushd bin/Tests > /dev/null

# Run tests
$exe_prefix "$script_path/packages/NUnit.ConsoleRunner.$NUNIT_VERSION/tools/nunit3-console.exe" --noresult --timeout=60000 --skipnontestassemblies --process=separate $@ \
    "$script_path/Source/CodeCompletion/Outracks.CodeCompletion.CodeNinja.Tests/bin/$CONFIG/Outracks.CodeCompletion.CodeNinja.Tests.dll" \
    "$script_path/Source/CodeCompletion/Outracks.CodeCompletion.UXNinja.Tests/bin/$CONFIG/Outracks.CodeCompletion.UXNinja.Tests.dll" \
    "$script_path/Source/Common/Tests/bin/$CONFIG/Outracks.Common.Tests.dll" \
    "$script_path/Source/Fuse/Tests/bin/$CONFIG/Outracks.Fuse.Protocol.Tests.dll" \
    "$script_path/Source/Fusion/IntegrationTests/bin/$CONFIG/Fusion-IntegrationTests.exe" \
    "$script_path/Source/Fusion/Tests/bin/$CONFIG/Outracks.Fusion.Tests.dll" \
    "$script_path/Source/Preview/Tests/bin/$CONFIG/Fuse.Preview.Tests.dll" \
    "$script_path/Source/Simulator/Tests/bin/$CONFIG/Outracks.Simulator.Tests.dll" \
    "$script_path/Source/UnoHost/Tests/bin/$CONFIG/Outracks.UnoHost.Common.Tests.dll"

popd > /dev/null
