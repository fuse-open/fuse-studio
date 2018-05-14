#!/bin/bash
trap 'echo BUILD FAILED!; exit 1' ERR
cd "`dirname "$0"`"

if [ "$OSTYPE" = msys ]; then
    cmd //c build.bat
    exit $?
fi

DO_INIT="0"
NO_BUILD="0"
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
    --init)
        DO_INIT="1"
        ;;
    --nobuild)
        NO_BUILD="1"
        ;;
    -h|--help)
        echo "Available options:"
        echo ""
        echo "    --init      Initializes git submodules and install nuget packages (auto first build)"
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

if [ "$DO_INIT" = "1" ]; then

    if [ "$IS_LINUX" = "1" ]; then
        # NuGet fails on Linux without this
        mozroots --import --ask-remove
    fi

    mono .nuget/NuGet.exe restore Fuse-OSX.sln

    mono Stuff/stuff.exe install Stuff 
    touch .build-inited
fi

if [ "$NO_BUILD" == "1" ]; then
    exit 0
fi

case $OSTYPE in
darwin*)
    msbuild Fuse-OSX.sln /p:Configuration=$CONFIGURATION
    ;;
*)
    echo "ERROR: Unsupported system '$OSTYPE'" >&2
    exit 1
    ;;
esac
