#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
source "scripts/common.sh"
shopt -s dotglob

VERSION=`bash scripts/get-version.sh`
COMMIT=`git rev-parse HEAD`
BIN="bin/Release"

echo "Version: $VERSION"
echo "Commit: $COMMIT"

# Validate package.json file.
node scripts/check-dependencies.js

# Extract the X.Y.Z part of version, removing the suffix if any.
VERSION_TRIPLET=`echo $VERSION | sed -n -e 's/\([^-]*\).*/\1/p'`

# We don't have a build-number.
BUILD_NUMBER=0

# Create GlobalAssemblyInfo.Override.cs.
sed -e 's/\(AssemblyVersion("\)[^"]*\(")\)/\1'$VERSION_TRIPLET.$BUILD_NUMBER'\2/' \
    -e 's/\(AssemblyFileVersion("\)[^"]*\(")\)/\1'$VERSION_TRIPLET.$BUILD_NUMBER'\2/' \
    -e 's/\(AssemblyInformationalVersion("\)[^"]*\(")\)/\1'$VERSION'\2/' \
    -e 's/\(AssemblyConfiguration("\)[^"]*\(")\)/\1'$COMMIT'\2/' \
    src/GlobalAssemblyInfo.cs > src/GlobalAssemblyInfo.Override.cs

# Sets default version number for Uno-libraries.
export npm_package_version=$VERSION

# Build assemblies.
if [ "$BUILD" = 0 ]; then
    echo "Skipping build because BUILD was set to 0."
else
    rm -rf "$BIN" > /dev/null
    bash scripts/build.sh --release --install
fi

# Remove GlobalAssemblyInfo.Override.cs.
rm -f src/GlobalAssemblyInfo.Override.cs

function find-all {
    local root="$1"
    shift
    while [ $# -gt 0 ]; do
        bash -lc "find \"$root\" -name \"$1\""
        shift
    done
}

function rm-all {
    IFS=$'\n'
    for i in `find-all "$@"`; do
        rm -rf "$i"
    done
}

function rm-empty {
    bash -lc "find \"$1\" -type d -empty -delete"
}

function filecompare {
    node_modules/.bin/filecompare "$i" "$file" | grep true > /dev/null
}

function rm-identical {
    local root=$1
    shift
    IFS=$'\n'
    for i in `find-all "$@"`; do
        local file="$root/`basename $i`"
        [ -f "$file" ] || continue
        filecompare "$i" "$file" || continue
        echo "stripping $file"
        rm -rf "$file"
        # Add placeholder for restore.js
        touch "$file.restore"
    done
}

function rm-identical2 {
    if [ "$OSTYPE" = msys ]; then
        rm-identical "$BIN" "$@"
    else
        rm-identical "$BIN" "$@"
        rm-identical "$BIN/fuse X.app/Contents/MonoBundle" "$@"
        rm-identical "$BIN/fuse X (menu bar).app/Contents/MonoBundle" "$@"
        rm-identical "$BIN"/UnoHost.app/Contents/MonoBundle "$@"
    fi
}

function rm-identical3 {
    if [ "$OSTYPE" = msys ]; then
        return
    fi

    for dst in \
        "$BIN" \
        "$BIN/fuse X (menu bar).app/Contents/MonoBundle" \
        "$BIN"/UnoHost.app/Contents/MonoBundle
    do
        rm-identical "$dst" "$BIN/fuse X.app/Contents/MonoBundle" "$@"
    done
}

function rm-identical4 {
    if [ "$OSTYPE" = msys ]; then
        return
    fi

    for dst in \
        "$BIN/fuse X (menu bar).app/Contents/Resources" \
        "$BIN/fuse X (uri-handler).app/Contents/Resources"
    do
        rm-identical "$dst" "$BIN/fuse X.app/Contents/Resources" "$@"
    done
}

h1 "Optimizing package"

# The following binaries will be added back by restore.js.
if [ "$OSTYPE" = msys ]; then
    rm-identical2 node_modules/@fuse-open/opentk *.dll
    rm-identical2 node_modules/@fuse-open/xamarin-mac *.dll
    rm-all "$BIN" *.dylib
else
    rm-identical2 node_modules/@fuse-open/xamarin-mac *.dll *.dylib
fi

rm-identical2 node_modules/@fuse-open/uno/bin *.dll *.dylib *.exe
rm-identical3 *.dll *.dylib *.exe
rm-identical4 *.icns

# Drop superfluous build artifacts.
rm-all "$BIN" FSharp.Core.resources.dll
rm-all "$BIN" *.config *.pdb *.xml
rm-all "$BIN" *.mdb *.pkg
rm-empty "$BIN"

# Error handling.
function error {
    echo "prepack failed."
    mv -f package.json-original package.json
    exit 1
}

trap 'error' ERR

# Update package.json file.
function set-platform {
    cp -f package.json package.json-original || exit 1
    node scripts/update-json.js package.json "$1" || error
}

case $OSTYPE in
darwin*)
    set-platform mac

    # Update version info.
    IFS=$'\n'
    for plist in `find-all "$BIN" *.plist`; do
        echo "updating version info in $plist"
        node_modules/.bin/replace "VERSION_NUMBER" "$VERSION" "$plist" > /dev/null
    done
    ;;

msys*)
    set-platform win

    if [ -z "$FUSE_CERTIFICATE_NAME" ]; then
        >&2 echo "WARNING: Executables were not signed (FUSE_CERTIFICATE_NAME not set)"
    else
        set --
        IFS=$'\n'
        for exe in `find-all "$BIN" *.exe`; do
            set -- "$@" "$exe"
        done
        "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\x64\\signtool.exe" \
            sign //a //n "$FUSE_CERTIFICATE_NAME" \
            //t http://timestamp.comodoca.com/authenticode \
            //v "$@"
    fi
    ;;
esac
