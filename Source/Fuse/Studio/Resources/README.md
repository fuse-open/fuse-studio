To create an `.icns` file from the command line:

Make a directory `<directory name>.iconset` containing the following icons:

```
icon_128x128.png
icon_128x128@2x.png
icon_16X16.png
icon_16X16@2x.png
icon_256x256.png
icon_256x256@2x.png
icon_32x32.png
icon_32x32@2x.png
icon_48x48.png
icon_48x48@2x.png
icon_512x512.png
icon_512x512@2x.png
```

Run the following command:

```
iconutil -c icns -o <file name>.icns <directory name>.iconset
```
