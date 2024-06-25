#!/bin/sh
set -e
cd "`dirname "$0"`/.."

if [ -z "$CONFIGURATION" ]; then
    CONFIGURATION="Debug"
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
nuget install NUnit.Console -version $NUNIT_VERSION -o packages

# The tests may create temporary files that we don't want git to find
script_path=`pwd`
mkdir -p bin/tests
pushd bin/tests > /dev/null

# Run tests
dotnet-run "$script_path/packages/NUnit.ConsoleRunner.$NUNIT_VERSION/tools/nunit3-console.exe" --noresult --timeout=60000 --skipnontestassemblies --process=separate $@ \
    "$script_path/src/ninja/Outracks.CodeCompletion.CodeNinja.Tests/bin/$CONFIGURATION/Outracks.CodeCompletion.CodeNinja.Tests.dll" \
    "$script_path/src/ninja/Outracks.CodeCompletion.UXNinja.Tests/bin/$CONFIGURATION/Outracks.CodeCompletion.UXNinja.Tests.dll" \
    "$script_path/src/common/Tests/bin/$CONFIGURATION/Outracks.Tests.dll" \
    "$script_path/src/fuse/Tests/bin/$CONFIGURATION/Outracks.Fuse.Protocol.Tests.dll" \
    "$script_path/src/fusion/IntegrationTests/bin/$CONFIGURATION/fusion-integrations.exe" \
    "$script_path/src/fusion/Tests/bin/$CONFIGURATION/Outracks.Fusion.Tests.dll" \
    "$script_path/src/preview/Tests/bin/$CONFIGURATION/Fuse.Preview.Tests.dll" \
    "$script_path/src/simulator/Tests/bin/$CONFIGURATION/Outracks.Simulator.Tests.dll" \
    "$script_path/src/unohost/Tests/bin/$CONFIGURATION/Outracks.UnoHost.Tests.dll"

popd > /dev/null
