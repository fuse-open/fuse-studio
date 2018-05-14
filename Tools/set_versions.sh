#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
SELF_DIR=`dirname "$SELF"`

source "Stuff/Devtools/utils.sh"

BUILD_COUNTER=$1
VERSION=$(cat VERSION.txt)
MAJOR=$(echo $VERSION | cut -d. -f1)
MINOR=$(echo $VERSION | cut -d. -f2)

echo "##teamcity[buildNumber '$VERSION+$1']"
echo "##teamcity[setParameter name='assemblyVersion' value='$MAJOR.$MINOR']"
echo "##teamcity[setParameter name='assemblyFileVersion' value='$VERSION.$BUILD_COUNTER']"
echo "##teamcity[setParameter name='releaseVersion' value='$VERSION']"

# A .teamcity_set_versions.sh may be contained in a repository containing
# commands that will be run during the "Set Version Number" step

DOT_SET_VERSIONS=".teamcity_set_versions.sh"

if [ -f "$DOT_SET_VERSIONS" ]; then
    source "$DOT_SET_VERSIONS"
fi
