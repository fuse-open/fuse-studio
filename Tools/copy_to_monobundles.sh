#!/bin/bash
set -e
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.."
cp -a Source/UnoHost/Mac/bin/Debug/* bin/Debug/Fuse.app/Contents/UnoHost.app/Contents/MonoBundle
cp -a Source/Fuse/Sandbox/bin/Debug/* Source/Fuse/Sandbox/Fuse\ Sandbox.app/Contents/MonoBundle
cp -a Source/Fuse/Studio/bin/Debug/* bin/Debug/Fuse.app/Contents/Fuse\ Studio.app/Contents/MonoBundle
cp -a Source/Fuse/Notifier/bin/Debug/* bin/Debug/Fuse.app/Contents/Fuse\ Tray.app/Contents/MonoBundle
cp -a Source/Outracks.Fuse.Startup-OSX/bin/Debug/* bin/Debug/Fuse.app/Contents/MonoBundle
cp -a Source/Fusion/IntegrationTests/bin/Debug/* Source/Fusion/IntegrationTests/Fusion-IntegrationTests.app/Contents/MonoBundle
