#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
set -e

export VERSION=`bash scripts/get-version.sh`

# Build NPM package
if [ "$PACK" = 0 ]; then
    echo "Skipping 'npm pack' because PACK was set to 0."
else
    bash scripts/prepack.sh
    npm pack --silent
    bash scripts/postpack.sh
    echo ""
fi

# Build installer
case $OSTYPE in
darwin*)
    rm -rf setup/pkg/root/tmp/*
    mkdir -p setup/pkg/root/tmp

    pushd setup/platypus > /dev/null
    platypus -P "fuse X.platypus" \
        --app-version "$VERSION" \
        "../pkg/root/tmp/fuse X.app"
    popd > /dev/null

    if [ -z "$FUSE_DEVELOPER_ID" ]; then
        >&2 echo "WARNING: App was not signed (FUSE_DEVELOPER_ID not set)"
    else
        codesign --force --deep --entitlements "setup/entitlements.plist" --options runtime \
            --timestamp --sign "Developer ID Application: $FUSE_DEVELOPER_ID" \
            "setup/pkg/root/tmp/fuse X.app"
    fi

    bash setup/codesign-tgz.sh fuse-x-studio-mac-$VERSION.tgz \
        setup/pkg/root/tmp/fuse-x-studio-mac.tar
    bash setup/pkg/build.sh

    UNSIGNED="setup/pkg/fuse-unsigned.pkg"
    INSTALLER="fuse-x-$VERSION-mac.pkg"

    if [ -z "$FUSE_DEVELOPER_ID" ]; then
        >&2 echo "WARNING: Installer was not signed (FUSE_DEVELOPER_ID not set)"
        mv $UNSIGNED $INSTALLER
    else
        productsign --sign "Developer ID Installer: $FUSE_DEVELOPER_ID" \
            $UNSIGNED $INSTALLER
        echo -e "\nNOTICE: Remember to notarize & staple the installer before publishing!"
    fi

    echo -e "\nBuilding DMG..."
    cp -f $INSTALLER "setup/dmg/Install fuse X.pkg"
    bash setup/dmg/build.sh

    UNSIGNED="setup/dmg/fuse-unsigned.dmg"
    INSTALLER="fuse-x-$VERSION-mac.dmg"

    if [ -z "$FUSE_DEVELOPER_ID" ]; then
        >&2 echo "WARNING: DMG was not signed (FUSE_DEVELOPER_ID not set)"
        mv "$UNSIGNED" "$INSTALLER"
    else
        mv "$UNSIGNED" "$INSTALLER"
        codesign --sign "Developer ID Application: $FUSE_DEVELOPER_ID" \
            "$INSTALLER"
        codesign "$INSTALLER" --display --verbose=2
        echo -e "\nNOTICE: Remember to notarize & staple the DMG before publishing!"
    fi
    ;;

msys*)
    cp fuse-x-studio-win-$VERSION.tgz setup/nsis
    setup/nsis/tools/makensis //DVERSION=$VERSION setup/nsis/fuse-setup.nsi
    INSTALLER="fuse-x-$VERSION-win.exe"
    mv setup/nsis/$INSTALLER $INSTALLER

    if [ -z "$FUSE_CERTIFICATE_NAME" ]; then
        >&2 echo "WARNING: Installer was not signed (FUSE_CERTIFICATE_NAME not set)"
    else
        "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\x64\\signtool.exe" \
            sign //a //n "$FUSE_CERTIFICATE_NAME" \
            //t http://timestamp.comodoca.com/authenticode \
            //v $INSTALLER
    fi
    ;;
esac

# Done
echo ""
echo "Your installer:"
echo "$INSTALLER"
echo "`du -k "$INSTALLER" | cut -f1` KB"
