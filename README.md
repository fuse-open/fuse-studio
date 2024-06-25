<p align="center"><picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/fuse-x/studio/raw/master/img/logo-darkmode.png">
  <img src="https://github.com/fuse-x/studio/raw/master/img/logo.png" width="216" alt="fuse X" />
</picture></p>

<p align="center"><img src="https://img.shields.io/badge/target%20os-Android%20%7C%20iOS%20%7C%20macOS%20%7C%20Windows-7F5AB6?style=flat-square&amp;logo=android&amp;logoColor=silver" alt="Target platforms">
<img src="https://img.shields.io/badge/host%20os-macOS%20%7C%20Windows-7F5AB6?logo=apple&amp;style=flat-square" alt="Host platforms">
<a href="https://github.com/fuse-x/studio/releases"><img src="https://img.shields.io/github/v/release/fuse-x/studio?include_prereleases&amp;logo=github&amp;label=latest&amp;sort=semver&amp;style=flat-square" alt="Latest version"></a>
<a href="https://github.com/fuse-x/studio/releases"><img src="https://img.shields.io/github/downloads/fuse-x/studio/total?logo=github&amp;color=blue&amp;style=flat-square" alt="Downloads"></a></p>

<p align="center"><img src="https://github.com/fuse-x/studio/blob/master/img/screenshot.png?raw=true" width="744" alt="fuse X" /></p>

> **fuse X** is a visual desktop tool suite for working with the [**Fuse Open**](https://fuseopen.com/) framework, on **macOS** and **Windows**.

## Releases

Official fuse X releases are published [here](https://github.com/fuse-x/studio).

## Build instructions

```
npm install
npm run build
npm run fuse
```

### Building on Windows

The prerequisites for building on Windows is

* [Visual Studio 2019](https://www.visualstudio.com/downloads/) - Community Edition
  * With .NET desktop development component installed
* [Git for Windows](https://git-scm.com/download/win)
* [Node.js](https://nodejs.org/)

Build by either running `npm run build` or from within Visual Studio using the `fuse-win.sln` solution.

After building fuse X can be started by running `npm run fuse` from the command line.

> To run with debugging in Visual Studio set `fuse-studio` as the startup project and press <kbd>F5</kbd>.

### Building on macOS

The prerequisites for building on macOS is

* [Xcode](https://developer.apple.com/xcode/)
  * Remember to open Xcode one time after installing to accept EULA
* [Node.js](https://nodejs.org/)
* [Mono](https://www.mono-project.com/download/stable/)

Build by either running `npm run build` or from within [Visual Studio for Mac](https://www.visualstudio.com/vs/mac/) using the `fuse-mac.sln` solution.

After building fuse X can be started by running `npm run fuse` from the command line.

> To run with debugging in Visual Studio for Mac set `fuse X` as the startup project and press <kbd>⌘</kbd>+<kbd>⏎</kbd>.

> Run `VSMAC=1 make` one time before launching from Visual Studio for Mac to prepare for debugging.

> Make sure you have the latest versions of Visual Studio for Mac, Mono and Xamarin.Mac installed.

## Running tests

```
npm test
```

## Building installers

```
npm run setup:build
```

## Using preview apps

Run the following command to build Uno libraries needed by preview apps.

```
npm run doctor
```

Run one of the following commands to start a preview app for your desired platform.

```
npm run app:android
npm run app:android-emu
npm run app:ios
npm run app:ios-sim
npm run app:native
```

> Unfortunately the preview app cannot be opened in fuse X.

Run one of the following commands to build a distributable Android APK or AAB.

```
npm run app:build-apk
npm run app:build-aab
```

## Upgrading Fuse Open components

We can use [npm-install](https://docs.npmjs.com/cli/install) to upgrade Uno and Fuselibs.

```
npm install --save @fuse-open/uno
npm install --save @fuse-open/fuselibs
npm install --save @fuse-open/types
```

We can see if any packages are outdated by running the following command.

```
npm outdated
```

## Incrementing the version number

We can use [npm-version](https://docs.npmjs.com/cli/version) to set a new version number.

```
npm version 1.2.3
```

> The first two major and minor parts of the version number should match the version numbers of Uno and Fuselibs.

Please add a suffix to the version number if making a pre-release.

```
npm version 1.2.3-canary.0
```

## Log files

Log files can be found at the following locations:

* macOS: `~/.fuse/logs/`
* Windows: `%LOCALAPPDATA%\fuse X\logs\`

Installer logs can be found at the following locations:

* macOS: `/tmp/`
