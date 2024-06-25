<p align="center"><picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/fuse-x/studio/raw/master/img/logo-darkmode.png">
  <img src="https://github.com/fuse-x/studio/raw/master/img/logo.png" width="216" alt="Fuse X" />
</picture></p>

<p align="center"><a href="https://ci.appveyor.com/project/fusetools/fuse-studio/branch/master"><img src="https://img.shields.io/appveyor/ci/fusetools/fuse-studio/master.svg?logo=appveyor&logoColor=silver&style=flat-square" alt="AppVeyor build status"></a>
<img src="https://img.shields.io/badge/target%20os-Android%20%7C%20iOS%20%7C%20macOS%20%7C%20Windows-7F5AB6?style=flat-square&amp;logo=android&amp;logoColor=silver" alt="Target platforms">
<img src="https://img.shields.io/badge/host%20os-macOS%20%7C%20Windows-7F5AB6?logo=apple&amp;style=flat-square" alt="Host platforms">
</p>

<p align="center"><a href="https://github.com/fuse-x/studio/releases"><img src="https://img.shields.io/github/v/release/fuse-x/studio?include_prereleases&amp;logo=github&amp;label=latest&amp;sort=semver&amp;style=flat-square" alt="Latest version"></a>
<a href="https://github.com/fuse-x/studio/releases"><img src="https://img.shields.io/github/downloads/fuse-x/studio/total?logo=github&amp;color=blue&amp;style=flat-square" alt="Downloads"></a>
<a href="LICENSE.txt"><img src="https://img.shields.io/github/license/fuse-open/fuse-studio.svg?logo=github&amp;style=flat-square" alt="License: MIT"></a>
<a href="https://fusecommunity.slack.com/"><img src="https://img.shields.io/badge/chat-on%20slack-blue.svg?logo=slack&amp;style=flat-square" alt="Slack"></a>
<a href="https://opencollective.com/fuse-open"><img src="https://opencollective.com/fuse-open/all/badge.svg?label=financial+contributors&amp;style=flat-square" alt="Financial Contributors on Open Collective"></a></p>

<p align="center"><img src="https://github.com/fuse-x/studio/blob/master/img/screenshot.png?raw=true" width="744" alt="Fuse X" /></p>

> **Fuse X** is a visual desktop tool suite for working with the [**Fuse Open**](https://fuseopen.com/) framework, on **macOS** and **Windows**.

## Releases

Official Fuse X releases are published [here](https://github.com/fuse-x/studio).

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

After building Fuse X can be started by running `npm run fuse` from the command line.

> To run with debugging in Visual Studio set `fuse-studio` as the startup project and press <kbd>F5</kbd>.

### Building on macOS

The prerequisites for building on macOS is

* [Xcode](https://developer.apple.com/xcode/)
  * Remember to open Xcode one time after installing to accept EULA
* [Node.js](https://nodejs.org/)
* [Mono](https://www.mono-project.com/download/stable/)

Build by either running `npm run build` or from within [Visual Studio for Mac](https://www.visualstudio.com/vs/mac/) using the `fuse-mac.sln` solution.

After building Fuse X can be started by running `npm run fuse` from the command line.

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

> Unfortunately the preview app cannot be opened in Fuse X.

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

## Contributing

> Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of
conduct, and the process for submitting pull requests to us.

### Reporting issues

Please report issues [here](https://github.com/fuse-open/fuse-studio/issues).

## Contributors

### Code Contributors

This project exists thanks to all the people who contribute. [[Contribute](CONTRIBUTING.md)]
<a href="https://github.com/fuse-open/fuse-studio/graphs/contributors"><img src="https://opencollective.com/fuse-open/contributors.svg?width=890&button=false" /></a>

### Financial Contributors

 Become a financial contributor and help us sustain our community. [[Contribute](https://opencollective.com/fuse-open/contribute)]

#### Individuals

<a href="https://opencollective.com/fuse-open"><img src="https://opencollective.com/fuse-open/individuals.svg?width=890"></a>

#### Organizations

Support this project with your organization. Your logo will show up here with a link to your website. [[Contribute](https://opencollective.com/fuse-open/contribute)]

<a href="https://opencollective.com/fuse-open/organization/0/website"><img src="https://opencollective.com/fuse-open/organization/0/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/1/website"><img src="https://opencollective.com/fuse-open/organization/1/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/2/website"><img src="https://opencollective.com/fuse-open/organization/2/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/3/website"><img src="https://opencollective.com/fuse-open/organization/3/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/4/website"><img src="https://opencollective.com/fuse-open/organization/4/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/5/website"><img src="https://opencollective.com/fuse-open/organization/5/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/6/website"><img src="https://opencollective.com/fuse-open/organization/6/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/7/website"><img src="https://opencollective.com/fuse-open/organization/7/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/8/website"><img src="https://opencollective.com/fuse-open/organization/8/avatar.svg"></a>
<a href="https://opencollective.com/fuse-open/organization/9/website"><img src="https://opencollective.com/fuse-open/organization/9/avatar.svg"></a>
