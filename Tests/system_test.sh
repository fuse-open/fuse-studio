#!/bin/bash
ROOT=$(pwd)"/"$(dirname "$0")"/.."

RUNNER="$ROOT/Tests/SystemTest/Debug/SystemTest.exe"
if [ "$OS" == "Windows_NT" ]; then
    MONO=""
    FUSE="$ROOT/bin/Debug/Fuse.exe"
else
    MONO="mono"
    FUSE="$ROOT/bin/Debug/Fuse.app/Contents/MacOS/Fuse"
fi

for arg in "$@"; do
    case $arg in
    --fuse=*)
        FUSE="${arg#*=}"
        shift
    ;;
    --runner=*)
        RUNNER="${arg#*=}"
        shift
    ;;
    --help|=h)
        echo "USAGE: $0 [--fuse=<Fuse.exe>] [--runner=<SystemTest.exe>] [params]"
        echo
        echo "If no options are given, it uses"
        echo "  --fuse=$FUSE"
        echo "  --runner=$RUNNER"
        echo
        echo "[params] are passed to the runner. Example:"
        echo "  --skip=auto-test-app"
        exit 0
    ;;
esac

done


$MONO $RUNNER --fuse=$FUSE $@
