#!/bin/sh
cd "$(dirname "$0")"

UNO="`pwd`/../../../Stuff/uno"

$UNO build $@ -DREFLECTION -DSIMULATOR -DDesignMode -DDESIGNER -DPREVIEW