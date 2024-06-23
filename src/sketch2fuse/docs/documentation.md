## Workflow
Because Fuse has a complex and flexible UI system, it's not a realistic goal to import a Sketch file and turn it into a complete Fuse app. Rather, we have aimed for a more meaningful integration with Sketch that leverages the best qualities of both programs. 

This integration will import your Sketch Symbols and interpret them into Fuse `ux:Classes`. Then allowing you to quickly assemble these components using Fuse's layout and animation into a prototype or app. Any changes made to the Symbols in Sketch, will be reflected in Fuse. This allows you to iterate extremely quickly on an idea. _Note: This integration is one-way ie: changes made in Sketch will reflect in Fuse, but changes made in Fuse won't reflect in Sketch. Your Sketch files will remain the source of truth for these components._ 

This workflow enables testing and iterating on by quickly creating prototypes. Once a direction has been decided, thoses components can be developed to be more dynamic and responsive to user input. 

## Getting Started

### Watching a sketch file. 
In order to make the integration work, there are a few steps to follow:
- Step1: In the same folder as your `.unoproj` file, you will need to create a file with the file-type `.sketchfiles`. This file will need to have the same name as your `.unoproj`. _For instance: `MyApp.unoproj.` will need `MyApp.sketchfiles`_
- Step 2: Inside this file you need to specify a list of paths to Sketch files you want to import. The file paths can be relative or absolut so you can link to a Sketch file anyway on your system. Here is an example:
```
[
  "MySketchFile.sketch", //Inside the app folder
  "Assets/MySketchFile.sketch", //Relative to the App folder
  "/Users/Me/SomeFolder/MySketchFolder/MySketchFile.sketch" //Absolute file path
]
```

Starting Fuse will import all the Symbols in the specified Sketch files into your project. The imported Classes have the naming convention `Sketch.ClassName.ux` to distinguish them from other `ux:Classes`. You will see them if you open Fuse Studio, under the Classes tab, or in the `SketchSymbols` folder that has been created in your project. You can include them in your app the same way you would any other class: `<Sketch.ClassName/>`

All changes saved to those Sketch files will reflect in Fuse.

## Best practices

### Editing generated UX
In the current iteration of the importer, any edits you make to the generated UX files will be overwritten the next time that Sketch file has been saved. Currently, the best way to break this link is to manually copy the UX into a new class with a different name and edit that. 

### Naming
- Make sure all your symbol names are unique. If there are two or more symbols with the same name, the importer will only import one of them.
- Symbol names cannot contain any special characters,symbols or spaces. Numbers and `_` are supported but the symbol name cannot begin with a number.
- If you want your text objects to be imported as a `ux:Property` (so you can re-use them in the same way as text overrides) you need to ensure that the layer names are unique. Text elements that have the same name won't be interpreted as a `ux:Property`, they will just be imported as plain Text elements.

## Known limitations

There are some aspects of Sketch that we either can't represent in UX or haven't yet added support for.

### General features
- **Library symbols** The importer can only interpret symbols from files included in the project. So if you have references to symbols in a seperate Library file which isn't included, those symbols won't be recognised. The solution is to include the central library file, or to `Detach Symbol` in Sketch manually.

### Layers
These properties are not supported on any type of layer (shapes, groups, images etc)
- **Export options** for all types of layers are ignored.
- **Blur** settings are currently ignored for all blurs (gaussian, motion, zoom and background blur).
- **Blending Modes** are currently not supported. Objects will be imported but the blending mode will be ignored.
- **Layer opacity** is currently not supported. Right now your only option for controlling transparency from Sketch is by tweaking color alpha values.
- **Masking** Fuse doesn't support masking in the same way Sketch does. Masked shapes are ignored.

### Fills
- **Single color** and **linear gradient** are the only supported fills at the moment. In other words: image, pattern, noise, radial gradient and angular gradients are not supported.

### Shapes
- **Compound / combined shapes**, shapes created with union, intersection and other boolean operators are not supported. A possible workaround is to flatten them to a sinle path when possible. Another option would be to create a copy of the shape and flatten it to a bitmap, which can then be used in Fuse
- **Rounded corners** on path/shape vertices are not supported except for on rectangles. If you need rounding on a general shape you can use `convert path to outlines` and manually round the corners in Sketch.
- **Border placement** (center, inside, outside) is ignored and the border is always centered.
- **Border styles** like ends, joins, start arrow, end arrow, dashes and gaps are not supported. Rounded end caps are not supported on all target platforms.

### Images
- **Color adjustment** of images are ignored.

### Text/font
- **Text on path** is not supported. The text and path are converted, but text doesn't follow the path.
- **Fonts** Using custom fonts in Fuse requires having copies of the `.ttf` or `.otf` files in your project folder. We don't currently support automatic adding of these fonts from the Sketch file to the projec folder, so all custom fonts get stripped in the conversion and the system font is used. You can manually set up a global font and apply that to text instances later.
- **Text styles / decoration** are ignored. Only text content, font size and color is converted.
- **Text shadows** currently not supported and also missing a proper warning.

### Shadows
- **Inner shadows** are not supported.

