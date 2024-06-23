#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
source "scripts/common.sh"

while [ $# -gt 0 ]; do
    case "$1" in
    -f|--fast)
        shift
        FAST=1
        ;;
    -i|--install)
        shift
        INSTALL=1
        ;;
    -r|--release)
        shift
        CONFIGURATION="Release"
        ;;
    *)
        break
        ;;
    esac
done

if [ -z "$CONFIGURATION" ]; then
    CONFIGURATION="Debug"
fi

if [ "$FAST" = 1 ]; then
    h1 "Building fuse X (fast mode)"

    if [ "$OSTYPE" = msys ]; then
        csharp-build fuse-win-dev.sln
    else
        csharp-build fuse-mac-dev.sln
    fi

    exit 0
fi

if [ ! -d node_modules/ ]; then
    INSTALL=1
elif [ ! -d packages/ ]; then
    INSTALL=1
fi

if [ "$INSTALL" = 1 ]; then
    h1 "Installing dependencies"
    npm install

    if [ "$OSTYPE" = msys ]; then
        nuget restore fuse-win.sln -Verbosity quiet
    else
        nuget restore fuse-mac.sln -Verbosity quiet
    fi
fi

h1 "Building libs"

uno doctor --configuration=$CONFIGURATION \
    src/simulator src/preview

h1 "Building fuse X"

if [ "$OSTYPE" = msys ]; then
    csharp-build fuse-win.sln
    exit 0
fi

# Remove old app bundles when debugging in VS Mac
if [ "$VSMAC" = 1 ]; then
    rm -rf bin/$CONFIGURATION/*.app
fi

csharp-build fuse-mac.sln

function cp2 {
    cp "$@" || exit $?
}

function assemble-app {
    local app=bin/$CONFIGURATION/$1.app
    local contents=$app/Contents
    local exe=$contents/MacOS/$1
    local monobundle=$contents/MonoBundle
    local resources=$contents/Resources

    mkdir -p "`dirname "$exe"`" "$monobundle" "$resources"
    cp2 src/mac/monostub/fonts.conf "`dirname "$exe"`"
    cp2 "$2"/Info.plist "$contents"
    cp2 "$2"/Resources/*.icns "$resources"

    # Skip the following steps when debugging in VS Mac
    if [ "$VSMAC" = 1 ]; then
        return
    fi

    # Replace executable
    cp2 src/mac/monostub/monostub "$exe"

    # Copy assemblies (since MMP is disabled in Xamarin.Mac.Common.targets)
    cp2 bin/$CONFIGURATION/*.{dll,exe} "$monobundle"

    # Remove system-installed Mono libraries from app bundle (#121)
    IFS=$'\n'
    for dll in `ls -1 /Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.6-api`; do
        if [ "$dll" = ICSharpCode.SharpZipLib.dll ]; then
            continue
        elif [ "$dll" = Mono.Options.dll ]; then
            continue
        elif [ "$dll" = Mono.Posix.dll ]; then
            continue
        elif [ "$dll" = System.Reactive.Windows.Threading.dll ]; then
            continue
        fi
        rm -rfv "$monobundle/$dll"
    done
    for dylib in `ls -1 /Library/Frameworks/Mono.framework/Versions/Current/lib`; do
        rm -rfv "$monobundle/$dylib"
    done

    # Copy WPF and Xamarin.Mac
    cp2 node_modules/@fuse-open/xamarin-mac/*.{dll,dylib} "$monobundle"
    cp2 src/mac/wpf/*.dll "$monobundle"
}

function create-exe {
    local dst=bin/$CONFIGURATION/$1
    cp2 src/mac/monostub/fonts.conf "`dirname "$dst"`"
    cp2 src/mac/monostub/monostub-console "$dst"
}

# Build monostub
pushd src/mac/monostub > /dev/null
make -s
popd > /dev/null

# Assemble app bundles
assemble-app "fuse X" src/mac/studio
assemble-app "fuse X (menu bar)" src/mac/menu-bar
assemble-app UnoHost src/unohost/mac

# Create native executables
create-exe fuse
create-exe fuse-lang
create-exe "fuse X.app/Contents/MonoBundle/fuse-preview"

# Copy UnoHost assemblies to main app (loaded using reflection)
cp bin/$CONFIGURATION/UnoHost.app/Contents/MonoBundle/*.{dll,exe} \
    "bin/$CONFIGURATION/fuse X.app/Contents/MonoBundle"

# Copy WPF assemblies (https://github.com/fuse-x/studio/issues/4)
cp src/mac/wpf/*.dll bin/$CONFIGURATION
