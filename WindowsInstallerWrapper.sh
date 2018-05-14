#!/bin/bash
set -e
cd "`dirname "$0"`"
cd "Installer/Windows"
pwd
echo $BUILD_NUMBER

case $OSTYPE in
msys*)
  ./BuildFinalInstaller.bat
  ;;
esac

if [ -n "$BUILD_NUMBER" ]; then
    VERSION="$BUILD_NUMBER"
else
    VERSION=$(cat ../../VERSION.txt)"-local"
fi

# replace + with . in BUILD_NUMBER
VERSION=$(echo $VERSION | sed 's/+/./')

# Rename output in BuildFinalInstaller 
mv BuildOutput/Release/FuseSetup.exe "BuildOutput/Release/Fuse_$VERSION.exe"



