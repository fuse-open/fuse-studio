This project contains the `BundleApp` MSBuild task for generating a macOS app bundle (`.app`) from a .net project.

The `monostub` executable is compiled from [monostub.m](monostub.m) and functions as the entry point for the generated app bundle. It supports using both bundled and installed mono, hence it looks for a file called `.mono_root` containing the relative path to the `mono` executable that is to be used.

The monostub also sets up the environment so that `mono` may find whatever it needs during runtime. It also sets the `rpath` to point to the containing directory of the `Mono` installation directory, which is the directory that the `Tools/mono_gen` tool by default sets the `install_name` of the native library dependencies to. 

