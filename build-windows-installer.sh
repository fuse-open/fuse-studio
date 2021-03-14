#!/bin/bash
set -e
cd "`dirname "$0"`"

OS_NAME="Win32"
VERSION=$(cat VERSION.txt)"-local"

DST="`pwd -P`/Release"
rm -rf "$DST"

CP="cp -f"
UNO_VER=`cat .unoversion`
UNO="$DST/Uno"
PACKAGES="$DST/Packages"
MODULES="$DST/Modules"
mkdir -p "$UNO" "$PACKAGES"

# Uno
echo "Copying Uno executables to '$UNO'"
$CP -R packages/FuseOpen.Uno.Tool.$UNO_VER/tools/* "$UNO"
$CP -R packages/FuseOpen.Uno.Tool.$UNO_VER/tools/.unoconfig "$UNO"

# Templates
echo "Copying Uno project templates to '$DST/Templates'"
mkdir -p "$DST/Templates"
$CP -R Templates/* "$DST/Templates"

# Modules
echo "Copying Sublime Text 3 plugin installer scripts to '$DST/Modules'"
mkdir -p "$DST/Modules"
$CP -R Modules/* "$DST/Modules"

# Warm up package installs to include what's needed to preview an app on .NET out-of-the-box.
echo "Fetching Uno packages needed for a regular app"
Stuff/uno create App -cApp -f
Stuff/uno build dotnet App
rm -rf App

# Packages
echo "Copying Uno packages to '$DST'"
$CP Stuff/*.packages "$DST"
$CP -R Stuff/lib/* "$PACKAGES"

# Fuse.unoconfig
echo "Write '$UNO/Fuse.unoconfig'"
cat <<EOF >> "$UNO/Fuse.unoconfig"

SimulatorDll: Fuse.Simulator.dll
TemplatesDirectory: Templates
ModulesDirectory: Modules

// Package manager config
Packages.InstallDirectory: "%LOCALAPPDATA%/Fusetools/Packages"
Packages.SearchPaths += Packages
Packages.LockFiles += [
    uno.packages
    fuselibs.packages
    premiumlibs.packages
]

// SDK config
SdkConfig: %LOCALAPPDATA%/Fusetools/Fuse/Android/.sdkconfig
include %LOCALAPPDATA%/Fusetools/Fuse/Android/.sdkconfig

//Extra unoconfig
include "%LOCALAPPDATA%/Fusetools/Fuse/extra.unoconfig"
EOF

mv "$UNO"/* "$UNO/.unoconfig" "$DST"
rm -rf "$UNO"

# Fuse.exe
echo "Copying Studio executables and libraries '$DST'"
$CP bin/Release/*.dll "$DST"
$CP bin/Release/*.exe "$DST"

# Simulator client uno project
echo "Copying Uno packages needed for preview to '$PACKAGES'"
$CP -r Source/Simulator/build/* "$PACKAGES"
$CP -r Source/Preview/build/* "$PACKAGES"

# Delete debug symbols to avoid bloating the installers
echo "Stripping debug symbol files"
find "$DST" -name "*.pdb" -exec rm {} \; || :
find "$DST" -name "*.mdb" -exec rm {} \; || :

echo "Building installer..."
cd "Installer"
cd "Windows"
cmd //c "BuildFinalInstaller.bat"

echo "All done!"