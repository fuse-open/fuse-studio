#!/bin/sh

set -e
cd "`dirname "$0"`"

# TeamCity sets $BUILD_NUMBER, so use that if set
if [ -n "$BUILD_NUMBER" ]; then
    VERSION="$BUILD_NUMBER"
else
    SHA=`git rev-parse HEAD`
    DATE=`date +"%y%m%d"`
    VERSION="$DATE-${SHA:0:6}"
fi
VERSION=`echo $VERSION | sed 's/\./_/g'`

# Clean up old stuff
rm -rf root pkgs *.pkg

if [ "$PACK" != "0" ]; then
    ../../pack.sh --no-zip "$@"
fi

# Create some directories
mkdir -p root/usr/local/bin
mkdir -p root/usr/local/share/uno
mkdir -p root/Applications

# Move stuff from Release instead of copying because it's faster
mv ../../Release/Packages root/usr/local/share/uno
mv ../../Release/Fuse.app root/Applications

# Move .packages files
mkdir -p root/Applications/Fuse.app/Contents
mv ../../Release/*.packages root/Applications/Fuse.app/Contents

# Set app permissions
ls -d1 root/Applications/*.app root/Applications/Fuse.app/Contents/*.app | while read app; do
    echo Setting permission for "$app"
    chmod -Rf +x "$app/Contents/MacOS/"
done

# Create symlinks for shared app binaries (Fuse.app reduced from 180MB to 60MB)
mono root/Applications/Fuse.app/Contents/Uno/uno.exe stuff symlink root/Applications/Fuse.app

if ! [ "$SIGN" = "0" ]; then
	codesign --force --deep -s "Developer ID Application: Outracks Technologies AS" root/Applications/Fuse.app
fi

# `uno` wrapper
cat <<'EOF' >> root/usr/local/bin/uno
#!/bin/sh
export FONTCONFIG_PATH=/Applications/Fuse.app/Contents/Mono/etc/fonts
exec /Applications/Fuse.app/Contents/Mono/bin/mono --gc=sgen /Applications/Fuse.app/Contents/Uno/uno.exe "$@"
EOF

cat <<'EOF' >> root/usr/local/bin/fuse
#!/bin/sh
/Applications/Fuse.app/Contents/MacOS/Fuse "$@"
EOF

# Remove all .DS_Store files
find root -name ".DS_Store" -depth -exec rm {} \;

# Make sure scripts are executable
chmod +x scripts/*
chmod +x root/usr/local/bin/*

mkdir -p pkgs
pkgbuild --root root \
    --component-plist components.plist \
    --identifier com.fusetools.fuse \
    --version 0.8 \
    --ownership recommended \
    --scripts scripts \
    pkgs/Fuse.pkg

UNSIGNED="Fuse_$VERSION-unsigned.pkg"
SIGNED="Fuse_$VERSION.pkg"

productbuild --distribution Distribution.xml \
    --package-path pkgs \
    --resources resources/ \
    $UNSIGNED

if [ "$SIGN" = "0" ]; then
    exit 0
fi

productsign --sign "Developer ID Installer: Outracks Technologies AS" \
    $UNSIGNED $SIGNED

spctl --assess --type install -v \
    $SIGNED
