To run system tests, do `./scripts/system-test.sh` from the repo root. Run with `-h` to see all available options.

To debug system tests, set this project as the startup project, and pass `--fuse=<path to locally built fuse>`. For example macOS: `--fuse="<repo root>/bin/Debug/fuse X.app/Contents/MacOS/fuse X"` and Windows: `--fuse=<repo root>\bin\Debug\fuse.exe`.
