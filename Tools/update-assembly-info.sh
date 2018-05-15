#!/bin/bash

set -e
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.."

function override-assembly-info {
	local version=$1
	# Extract the X.Y.Z part of version, removing the suffix if any
	local version_triplet=`echo $version | sed -n -e 's/\([^-]*\).*/\1/p'`

	# Start with updating the local GlobalAssemblyInfo.Override.cs
	sed -e 's/\(AssemblyVersion("\)[^"]*\(")\)/\1'$version_triplet'\2/' \
		  -e 's/\(AssemblyFileVersion("\)[^"]*\(")\)/\1'$version_triplet'\2/' \
		  -e 's/\(AssemblyInformationalVersion("\)[^"]*\(")\)/\1'$version'\2/' \
		  Source/GlobalAssemblyInfo.cs > Source/GlobalAssemblyInfo.Override.cs
}

if [ -n "$1" ]; then
	override-assembly-info "$1"
else
	echo "Usage: $0 <version>"
	exit 1
fi

