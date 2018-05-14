To run system tests, do `./Tests/system_test.sh` from the repo root. Run with `-h` to see all available options.

To debug system tests, set this project as the startup project, and pass `--fuse=<path to locally built fuse>`. For example macOS: `--fuse=<repo root>/bin/Debug/Fuse.app/Contents/MacOS/Fuse` and Windows: `--fuse=<repo root>\bin\Debug\Fuse.exe`.
