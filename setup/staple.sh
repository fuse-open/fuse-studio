#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
set -e

export VERSION=`bash scripts/get-version.sh`

if [[ "$OSTYPE" != darwin* ]]; then
    >&2 echo "ERROR: This operation is only supported on macOS"
    exit 1
fi

xcrun stapler staple fuse-x-$VERSION-mac.pkg
spctl --assess --type install -v fuse-x-$VERSION-mac.pkg

xcrun stapler staple fuse-x-$VERSION-mac.dmg
spctl -a -t open --context context:primary-signature -v fuse-x-$VERSION-mac.dmg

echo ""
echo "All done - you can now distribute the signed, notarized and stapled installer or DMG!"
