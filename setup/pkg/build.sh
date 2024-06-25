#!/bin/bash
set -e
cd "`dirname "$0"`"

# Clean up old stuff
rm -rf pkgs *.pkg

# Remove all .DS_Store files
find root -name ".DS_Store" -depth -exec rm {} \;

# Make sure scripts are executable
chmod +x scripts/* *.sh

# Include installer scripts
cp check-system.sh root/tmp/
cp install-fuse.sh root/tmp/
cp launch-fuse.sh root/tmp/
cp *.js root/tmp/

mkdir -p pkgs
pkgbuild --root root \
    --identifier com.fuse-x.studio \
    --version $VERSION \
    --ownership recommended \
    --scripts scripts \
    pkgs/fuse-studio.pkg

productbuild --distribution Distribution.xml \
    --package-path pkgs \
    --resources resources/ \
    fuse-unsigned.pkg
