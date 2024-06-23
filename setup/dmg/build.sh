#!/bin/bash
set -e
cd "`dirname "$0"`"

rm -rf fuse-unsigned.dmg
npx appdmg dmg.json fuse-unsigned.dmg
./seticon dmg.icns fuse-unsigned.dmg
