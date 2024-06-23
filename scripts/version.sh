#!/bin/bash
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`/.." || exit 1
set -e

# Most of the following code was copied from pack.sh.
#####################################################

# Detect version info
VERSION=`bash scripts/get-version.sh`-dev
BUILD_NUMBER="0"

# Extract the X.Y.Z part of version, removing the suffix if any
VERSION_TRIPLET=`echo $VERSION | sed -n -e 's/\([^-]*\).*/\1/p'`

# Create GlobalAssemblyInfo.Override.cs
sed -e 's/\(AssemblyVersion("\)[^"]*\(")\)/\1'$VERSION_TRIPLET.$BUILD_NUMBER'\2/' \
    -e 's/\(AssemblyFileVersion("\)[^"]*\(")\)/\1'$VERSION_TRIPLET.$BUILD_NUMBER'\2/' \
    -e 's/\(AssemblyInformationalVersion("\)[^"]*\(")\)/\1'$VERSION'\2/' \
    src/GlobalAssemblyInfo.cs > src/GlobalAssemblyInfo.Override.cs

echo "Updating GlobalAssemblyInfo to version $VERSION"
cat src/GlobalAssemblyInfo.Override.cs > src/GlobalAssemblyInfo.cs
rm src/GlobalAssemblyInfo.Override.cs

# Stage for commit.
git add src/GlobalAssemblyInfo.cs
