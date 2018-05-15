#!/bin/bash
set -e
cd "`dirname "$0"`"

if [ -n "$RELEASE_VERSION" ]; then
    VERSION="$RELEASE_VERSION"
else
    VERSION=$(cat VERSION.txt)"-local"
fi

DST="`pwd -P`/Release"
rm -rf "$DST"

DO_BUILD="1"
DO_ZIP="1"

for arg in "$@"; do
    case $arg in
    --no-build)
        DO_BUILD="0"
        ;;
    --no-zip)
        DO_ZIP="0"
        ;;
    *)
        echo "ERROR: Invalid argument '$arg'" >&2
        exit 1
        ;;
    esac
done

if [ "$DO_BUILD" = "1" ]; then
    if [ "$OSTYPE" = "msys" ]; then
        cmd //c build.bat --release
    else
        ./build.sh --release
    fi

    # Warm up package installs to include what's needed to preview an app on .NET out-of-the-box.
    Stuff/uno create App -cApp -f
    Stuff/uno build dotnet App
fi

echo "Copying files to '$DST'"

if [ "$OSTYPE" = "msys" ]; then
    CP="cp -f"
else
    CP="cp -f"
fi

UNO_VER=`cat .unoversion`
UNO="$DST/Uno"
PACKAGES="$DST/Packages"
MODULES="$DST/Modules"
mkdir -p "$UNO" "$PACKAGES"

# Uno
$CP -R packages/FuseOpen.Uno.Tool.$UNO_VER/tools/* "$UNO"
$CP -R packages/FuseOpen.Uno.Tool.$UNO_VER/tools/.unoconfig "$UNO"

# Templates
mkdir -p "$DST/Templates"
$CP -R Templates/* "$DST/Templates"

# Modules
mkdir -p "$DST/Modules"
$CP -R Modules/* "$DST/Modules"

# Packages
$CP Stuff/*.packages "$DST"

# Include installed packages in installer
$CP -R Stuff/lib/* "$PACKAGES"

# Fuse.unoconfig
if [ "$OSTYPE" = msys ]; then
    PACKAGE_INSTALL_DIR=%LOCALAPPDATA%/Fusetools/Packages
else
    PACKAGE_INSTALL_DIR="%HOME%/Library/Application Support/Fusetools/Packages"
fi

cat <<EOF >> "$UNO/Fuse.unoconfig"

if WIN32 {
    SimulatorDll: Fuse.Simulator.dll
} else {
    SimulatorDll: MonoBundle/Fuse.Simulator.dll
}
TemplatesDirectory: Templates
ModulesDirectory: Modules
Mono: Mono/bin/mono

// Package manager config
Packages.InstallDirectory: "$PACKAGE_INSTALL_DIR"
Packages.SearchPaths += Packages
Packages.LockFiles += [
    uno.packages
    fuselibs.packages
]

// SDK config
if WIN32 {
    SdkConfig: %LOCALAPPDATA%/Fusetools/Fuse/Android/.sdkconfig
    include %LOCALAPPDATA%/Fusetools/Fuse/Android/.sdkconfig
} else {
    SdkConfig: "%HOME%/Library/Application Support/Fusetools/Fuse/Android/.sdkconfig"
    include "%HOME%/Library/Application Support/Fusetools/Fuse/Android/.sdkconfig"
}

//Extra unoconfig
if WIN32 {
    include "%LOCALAPPDATA%/Fusetools/Fuse/extra.unoconfig"
} else {
    include "%HOME%/.fuse/extra.unoconfig"
}
EOF

if [ "$OSTYPE" != msys ]; then
cat <<EOF >> "$UNO/Fuse.unoconfig"
// Simulator packages
Packages.SearchPaths += /usr/local/share/uno/Packages
EOF
fi

case $OSTYPE in
darwin*)
    OS_NAME="OSX"

    # Fix Info.plist
    PLIST=bin/Release/Fuse.app/Contents/Info.plist
    sed -i '' "s/VERSION_NUMBER/$VERSION/" $PLIST
    sed -i '' 's/Copyright [^<]*/Copyright © 2017 Fusetools AS/' $PLIST

    # Simulator client uno project
    rsync -ru --copy-links Source/Simulator/build/* "$PACKAGES"
    rsync -ru --copy-links Source/Preview/build/* "$PACKAGES"

    # Fuse.app
    rsync -r --copy-links bin/Release/Fuse.app "$DST"

    # Bundle Uno, Templates, Modules and Tools inside Fuse.app
    mv "$UNO/Fuse.unoconfig" "$DST/Templates" "$DST/Modules" "$DST/Fuse.app/Contents"

    mkdir -p "$DST/Fuse.app/Contents/Uno"
    mv "$UNO"/* "$UNO/.unoconfig" "$DST/Fuse.app/Contents/Uno"

    rm -rf "$UNO" "$DST/Templates" "$DST/Modules"

cat <<EOF >> "$DST/Fuse.app/Contents/.unoconfig"
Packages.SearchPaths += ../../Packages
include Uno/.unoconfig
include Fuse.unoconfig
EOF

    ;;

linux*)
    OS_NAME="Linux"
    ;;

msys*)
    OS_NAME="Win32"
    mv "$UNO"/* "$UNO/.unoconfig" "$DST"
    rm -rf "$UNO"

    # Fuse.exe
    $CP bin/Release/*.dll "$DST"
    $CP bin/Release/*.exe "$DST"
    $CP bin/Release/fuse_simulator.ico "$DST"

    # Simulator client uno project
    $CP -r Source/Simulator/build/* "$PACKAGES"
    $CP -r Source/Preview/build/* "$PACKAGES"
    ;;
esac

# Delete debug symbols to avoid bloating the installers
find "$DST" -name "*.pdb" -exec rm {} \; || :
find "$DST" -name "*.mdb" -exec rm {} \; || :

if [ "$DO_ZIP" = "1" ]; then

    ZIP="`pwd -P`/Fuse-$VERSION-$OS_NAME.zip"

    rm -f "$ZIP"
    cd "$DST"
    if [ "$OSTYPE" = "msys" ]; then
        "C:\Program Files\7-Zip\7z.exe" a -y "$ZIP" * .unoconfig
    else
        zip -r --symlinks "$ZIP" * .unoconfig
    fi

    echo -e "\nRESULT: $ZIP"

fi
