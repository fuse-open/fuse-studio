# Build tasks
This project is supposed to contain different build tasks that are used during compilation of Fuse.

## BundleTask
This task generates a macOS application (`.app`) which supports use of bundled and installed mono. Example of usage can be found [here](https://github.com/fusetools/Fuse/blob/62b4ebb0d59e434a3e47746b2a359dc51a006dcc/Source/Fuse/Main/Outracks.Fuse.Main.csproj#L447).

The native-application is based on the [monostub.m](monostub.m). The monostub looks for a file called `.mono_root` which specifies a relative path to where mono is supposed to be installed, and that's how the right mono is located. The monostub also sets up the environment so that `mono` may find whatever it needs during runtime. It also sets the `rpath` to point to the containing directory of the `Mono` installation directory, which is the directory that the `Tools/mono_gen` tool by default sets the `install_name` of the native library dependencies to. 

