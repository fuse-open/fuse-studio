#!/bin/bash
#
# This scripts unpacks the TGZ, signs all binaries, and repacks as TAR.
#
# The reason we are doing this is that we get the error "The signature of
# the binary is invalid." if we sign binaries prior to creating the TGZ
# using 'npm pack'. Apparently doing that results in corrupted signatures
# that Apple's notarization service is not able to verify.
#
set -e

# Arguments
SRC=$1
DST=$2

if [[ -z "$SRC" || -z "$DST" ]]; then
    >&2 echo "ERROR: Missing arguments"
    exit 1
fi

# Script directory
DIR=`pwd`/`dirname "$0"`

# Unpack
rm -rf temp
mkdir -p temp
echo "unpacking $SRC"
tar -xzf "$SRC" -C temp
pushd temp > /dev/null

if [ -z "$FUSE_DEVELOPER_ID" ]; then
    >&2 echo "WARNING: Binaries were not signed (FUSE_DEVELOPER_ID not set)"
else
    # Sign executables
    IFS=$'\n'
    for exe in \
            "fuse" \
            "fuse-lang" \
            "fuse X.app/Contents/MonoBundle/fuse-preview"; do
        echo "signing $exe"
        codesign --force --entitlements "$DIR/entitlements.plist" --options runtime \
            --timestamp --sign "Developer ID Application: $FUSE_DEVELOPER_ID" \
            "package/bin/Release/$exe"
    done

    # Sign app-bundles
    IFS=$'\n'
    for app in \
            "fuse X (menu bar).app" \
            "fuse X.app" \
            "UnoHost.app"; do
        echo "signing $app"
        codesign --force --deep --entitlements "$DIR/entitlements.plist" --options runtime \
            --timestamp --sign "Developer ID Application: $FUSE_DEVELOPER_ID" \
            "package/bin/Release/$app"
    done
fi

# Repack
echo "creating $DST"
tar -cf package.tar package/
popd > /dev/null

# Done
mv -f temp/package.tar "$DST"
rm -rf temp
