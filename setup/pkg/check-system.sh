#!/bin/bash
set -e

MIN_MONO_VERSION=6.4
MIN_NODE_VERSION=10.0
MIN_NPM_VERSION=6.0

ROOT=`dirname "$0"`

# Mono and NPM
export PATH=/usr/local/bin:/opt/local/bin:/opt/homebrew/bin:$PATH
export PATH=/Library/Frameworks/Mono.framework/Versions/Current/Commands:$PATH

function check-version {
    # Test that command works
    "$1" --version > /dev/null 2> /dev/null || return 1

    local version=`"$1" --version | node "$ROOT/get-version.js"`
    if [ -z "$version" ]; then
        echo -e "Failed to detect version of '$1'." >&2
        return 0
    fi

    if node "$ROOT/version-gte.js" "$version" "$2"; then
        echo "using $1 version $version"
        return 0
    fi

    return 1
}

function error-dialog {
    osascript -e "tell application (path to frontmost application as text) to display dialog \"$1\" buttons {\"OK\"} with icon stop"
    exit 1
}

function check-version2 {
    check-version "$1" "$2" || error-dialog "This application requires '$1' version $2 or newer.\\n\\nPlease install the latest version of '$1' and try again.\\n\\n$3"    
}

check-version2  mono    $MIN_MONO_VERSION   https://www.mono-project.com/download/
check-version2  node    $MIN_NODE_VERSION   https://nodejs.org/en/download/
check-version2  npm     $MIN_NPM_VERSION    https://nodejs.org/en/download/

if [ -f ~/.dotnet-run/.bin/mono ]; then
    check-version ~/.dotnet-run/.bin/mono $MIN_MONO_VERSION || rm -rf ~/.dotnet-run
fi
