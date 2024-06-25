#!/bin/bash
trap 'echo BUILD FAILED!; exit 1' ERR
cd "`dirname "$0"`"

CONFIGURATION="Debug"

# Automatically init on first run
if [ ! -f .build-inited ]; then
    DO_INIT="1"
fi

for arg in "$@"; do
    case $arg in
    --debug)
        CONFIGURATION="Debug"
        ;;
    --release)
        CONFIGURATION="Release"
        ;;
    -h|--help)
        echo "Available options:"
        echo ""
        echo "    --release   Uses release configuration"
        echo "    --debug     Uses debug configuration (default)"
        echo ""
        exit 0
        ;;
    *)
        echo "ERROR: Invalid argument '$arg'" >&2
        exit 1
        ;;
    esac
done


if [ "$OSTYPE" = msys ]; then
    BAT_ARGUMENT="--debug"
    if [ "$CONFIGURATION" == "Release" ]; then
        BAT_ARGUMENT="--release"
    fi
    cmd //c build.bat $BAT_ARGUMENT
    exit $?
fi

nuget restore

MSBUILD="/Library/Frameworks/Mono.framework/Versions/4.4.2/bin/msbuild"
if [ ! -f "$MSBUILD" ]; then
    echo "ERROR: '$MSBUILD' was not found. Please install: http://download.mono-project.com/archive/4.4.2/macos-10-universal/MonoFramework-MDK-4.4.2.11.macos10.xamarin.universal.pkg" >&2
    exit 1
fi

"$MSBUILD" Sketch2Fuse.sln /p:Configuration=$CONFIGURATION
