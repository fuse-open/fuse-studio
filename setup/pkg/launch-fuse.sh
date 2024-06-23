#!/bin/bash
set -e

# Mono and NPM
export PATH=/usr/local/bin:/opt/local/bin:/opt/homebrew/bin:$PATH
export PATH=/Library/Frameworks/Mono.framework/Versions/Current/Commands:$PATH

# Restart fuse X
/usr/local/bin/fuse kill-all
/usr/local/bin/fuse
