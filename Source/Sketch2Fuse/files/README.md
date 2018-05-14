## Sketch test files

### How to add test file
The regression tests currently converts symbols for all sketch files
under the folder `files/Sketch43`: 

1. Open Sketch and create an example
1. Create a symbol for each part you want the converter to process
1. All symbols are saved in a separate Symbols page, to reduce noise you can remove the other pages in the document
1. Run the regression test with the `-i`-option. Accept all changes for the new file
1. Commit both sketch-file and reference files produced by the regression test.

Regression tests can be run with the command 

`mono ./RegressionTest/bin/Debug/RegressionTest.exe -i`

**Make sure you've built the regression test project so it uses the correct converter executable.**

### sketch43-file-unpacking.py

This script unpacks all Sketch-files in the `Sketch43`-folder. Each sketch
file becomes a subfolder under `files/output`. This is a development tool
for inspecting the json-files that are zipped into the sketch zip-file.

