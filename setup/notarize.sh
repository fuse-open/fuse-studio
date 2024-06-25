#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
set -e

export VERSION=`bash scripts/get-version.sh`

if [[ "$OSTYPE" != darwin* ]]; then
    >&2 echo "ERROR: This operation is only supported on macOS"
    exit 1
fi

if [[ -z "$FUSE_APPLEID_USER" || -z "$FUSE_APPLEID_PASS" || -z "$FUSE_TEAM_ID" ]]; then
    >&2 echo "ERROR: Please set FUSE_APPLEID_USER, FUSE_APPLEID_PASS and FUSE_TEAM_ID first"
    exit 1
fi

xcrun altool --notarize-app -f fuse-x-$VERSION-mac.pkg \
    -u "$FUSE_APPLEID_USER" -p "$FUSE_APPLEID_PASS" \
    --primary-bundle-id com.fuse-x.studio \
    --team-id "$FUSE_TEAM_ID"

xcrun altool --notarize-app -f fuse-x-$VERSION-mac.dmg \
    -u "$FUSE_APPLEID_USER" -p "$FUSE_APPLEID_PASS" \
    --primary-bundle-id com.fuse-x.studio \
    --team-id "$FUSE_TEAM_ID"

echo "Use the following command to check notarization status:"
echo ""
echo "    xcrun altool --notarization-info REQUEST_UUID --username \"\$FUSE_APPLEID_USER\" --password \"\$FUSE_APPLEID_PASS\" --team-id \"\$FUSE_TEAM_ID\""
echo ""
echo "Once OK, use the following command to staple the notarization status to our installer:"
echo ""
echo "    npm run setup:staple"
echo ""
