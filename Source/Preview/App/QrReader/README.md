Fork of https://github.com/zean00/fuse-qreader/commit/2768123269b7844232bc499dff8a9bc49b089e6f

Fuse Qreader
============

Library qr code scanner for [Fuse](http://www.fusetools.com/).

This library is adaptation (the structure and code base) of Fuse Gallery (https://github.com/bolav/fuse-gallery)
The QR Code reader is using built iOS framework and the code is based on sample code by Yannick Loriot (https://github.com/yannickl/QRCodeReaderViewController)

The android version is based on google vision sample that use Google Vision API (Part of Google Play Service library) (https://github.com/googlesamples/android-vision)

## Dependency (Android)
* Android Support Repository
* Google Play Services

## Installation

## Build

Android 
```
fuse build -t=android -r Example/Example.unoproj
```

## Usage

UX:

```
<Qreader ux:Global="Qreader" />
<Text Value="{txt}">
```

JavaScript:

```
var qreader = require('Qreader');
var Observable = require('FuseJS/Observable');
var txt = Observable();

function load () {
 qreader.scan().then(function (res) {
  txt.value = res;
 });
}
module.exports = {
 load: load,
 txt: txt
}
```
