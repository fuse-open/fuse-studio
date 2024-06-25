## Building

### Windows
We use Visual Studio 2015 and 2017 for development, but other IDEs might also work.

Use `build.bat` to build from the command line. (`build.sh` can also be used, it just delegates to `build.bat` on Windows)

### macOS
Mono 4 is required, we normally use 4.4.2. We use [Rider](https://www.jetbrains.com/rider/) for development, but other IDEs might also work.

Use `build.sh` to build from the command line

## Running
There's a command line tool meant for internal use that runs the converter on a Sketch file. Run it without parameters for usage information:

```
Command/bin/Debug/Command.exe
```

## Testing
There are two types of tests, NUnit tests and the regression tests. A shortcut `./test.sh` will run all of them.

### NUnit tests
These are the normal unit and system tests.

We recommend running these with the test runner in your favorite IDE, but they are also run by the shortcut `./test.sh`

### Regression tests
The regression tests run the converter on a set of known `.sketch` files and compare the results to reference `.ux` files. In case of any differences, the test fails. The user can then select whether the new result is correct, and replace the reference `.ux` file.

This can be run by `RegressionTests/bin/Debug/RegressionTests.exe`, but is also run by the shortcut `./test.sh`. Run  `RegressionTests/bin/Debug/RegressionTests.exe -h` for help.

## Distribution

### Versioning
We're using [GitVersion](http://gitversion.readthedocs.io) for versioning.

#### Official releases
To make an official release, simply create a tag with the desired version number. Remember to push the tag. This can be done from any branch, GitVersion only cares about the tag. Early on in the project we'll just release from master, but as the project gets more serious we'll release from proper release branches.

##### Example
Do `git tag 0.2.0` and `git push origin 0.2.0`. When you trigger this in TeamCity from master or a release branch, it gets version `0.2.0`, which also ends up as the version of the NuGet package.

#### Arbitrary releases from master / feature branches
Some times you need a NuGet package from a feature branch or master, to test in Fuse. Any time a build triggers from an untagged commit, GitVersion computes a version number automatically, which gets set in the NuGet package.

##### Examples
Say the last tagged release was 0.2.0, and your branch is some commits ahead of this tag. New versions automatically get bumped to 0.2.1, with the branch name and a pre-release number appended. For instance, the branch `feature/foo` gets versions like `0.2.1-foo001`, `0.2.1-foo002` and so on. `master` gets versions like `0.2.1-master0001`.

#### Pre releases from release branches
Again, GitVersion computes a version number automatically, which gets set in the NuGet package.

##### Examples
Say you're on a release branch `release/0.3`, but haven't tagged a commit as the official `0.3.0` yet. It then gets versions like `0.3.0-beta0000`, `0.3.0-beta0001` etc.

### NuGet
TeamCity automatically creates NuGet packages, which get published to its internal feed. They get their versions set automatically by the versioning scheme mentioned above.

### Upgrading the version used in Fuse
To upgrade the version of Sketch2Fuse used in Fuse, simply switch the version specified in `src/Fuse/Studio/packages.config`, and it will download the correct package from TeamCity's NuGet feed.
