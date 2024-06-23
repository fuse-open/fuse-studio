#!/bin/sh
SELF=`echo $0 | sed 's/\\\\/\\//g'`
cd "`dirname "$SELF"`" || exit 1
npx uno build $@ -DREFLECTION -DSIMULATOR -DDESIGNER -DPREVIEW