# Fuse Studio

Fuse Studio is a visual desktop tool suite for working with the Fuse framework.
 
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![Windows build status](https://ci.appveyor.com/api/projects/status/github/fuse-open/fuse-studio?branch=master&svg=true)](https://ci.appveyor.com/project/fusetools/fuse-studio/branch/master) [![macOS build Status](https://travis-ci.org/fuse-open/fuse-studio.svg)](https://travis-ci.org/fuse-open/fuse-studio)

For download links and docs click [here](http://fuse-open.github.io/).

## Building on Windows

The prerequisites for building on Windows is

* [Visual Studio 2017](https://www.visualstudio.com/downloads/) - Community Edition works fine
  * With .NET desktop development component installed
* [Node.js](https://nodejs.org/)
* [Git for Windows](https://git-scm.com/download/win)

Build by either running `build.bat` or from within Visual Studio using the `Fuse-Win32.sln` solution.

To run with debugging in Visual Studio set `Outracks.Fuse.Studio` as the startup project and press F5.

## Building on macOS

The prerequisites for building on macOS is

* [XCode](https://developer.apple.com/xcode/)
  * Remember to open XCode one time after installing to accept EULA
* [Mono](https://www.mono-project.com/download/stable/)
  * Tested with [5.4.1](https://download.mono-project.com/archive/5.4.1/macos-10-universal/MonoFramework-MDK-5.4.1.7.macos10.xamarin.universal.pkg), but newer versions should also work fine

Build by either running `./build.sh` from a shell, or from within [Visual Studio for Mac](https://www.visualstudio.com/vs/mac/) using the `Fuse-OSX.sln` solution.

After building Fuse Studio can be started by running `bin/Debug/Fuse.app/Contents/Fuse\ Studio.app/Contents/MacOS/Fuse\ Studio` from the command line.

## Running tests

Tests can be run using `./run-tests.sh`.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

### Reporting issues

Please report issues [here](https://github.com/fuse-open/fuse-studio/issues).

## Building installers

Note that building installers is due to legacy reasons a bit cumbersome, and we eventually want that part of the build process cleaned up.

### Making a macOS installer

Note that the current installer scripts expects a signing key available in the keychain, which is the property of Fusetools AS. It should be easy to modify script to use a different signing key if desirable.
By setting env var `SIGN` to `0` it should be possible to just create an unsigned installer, but haven't tested that this works.

To give the installer a specific version, set the environment vars RELEASE_VERSION and BUILD_NUMBER (this used to be set by the old CI setup when running on TC).

```shell
git clean -xdf && RELEASE_VERSION=1.9.0-rc3 BUILD_NUMBER=1.9.0-rc3 Installer/OSX/build.sh
```

The above commands produces an installer located at `Installer/OSX/Fuse_1_9_0-rc3.pkg`. We'll rename this to `fuse_osx_1_9_0_rc2.pkg` before uploading the release, to be consistent with naming of older versions.

The reason we do a `git clean` before building installer, is to avoid accidentially include cruft from older builds.

### Making a Windows installer

Making the Windows installer for the 1.9.0-rc3 release was done using the following commands (in a git bash shell).
Note that in addition to the regular build requirements this also requires [7-zip](https://www.7-zip.org/) installed.

```shell
 ( git clean -xdf && RELEASE_VERSION=1.9.0-rc3 BUILD_NUMBER=1.9.0-rc3 ./pack.sh && mkdir -p Installer/Windows/Source/Fuse && unzip Fuse-1.9.0-rc3-Win32.zip -d Installer/Windows/Source/Fuse/ && RELEASE_VERSION=1.9.0-rc3 BUILD_NUMBER=1.9.0-rc3 ./WindowsInstallerWrapper.sh )
```

 Note that we've dropped signing of the Windows installer, which seems to work fine. _If_ this causes more problems down the line than anticipated we might reconsider.

## Use Sublime or Atom plugin with dev build

To set which fuse to start in sublime, can be done by setting `fuse_path_override: false` inside your settings file. Open your settings by clicking Preferences->Package Settings->Fuse->Settings-User.

To set which fuse to start in Atom, is done by setting:
![Fuse settings](http://az664292.vo.msecnd.net/files/I4UI3gJqReq1fpI6-atom_2016-01-22_18-33-41.png)
