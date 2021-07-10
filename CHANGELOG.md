- Reflect icons from Fernando Lins on Fuse Open project [logo contest](https://fuseopen.com/logo-contest/)
- Improved problems view
- Remove old sketch importer, import Sketch symbols using the new Sketch importer instead. Add Sketch files to watch in `Fuse Studio` from `Project/Sketch import`-dialog or use command line `fuse import <file>.sketch` to import symbols from a single Sketch file. `fuse import <project>.sketchFiles` will import symbols from all files added from `Fuse Studio`. 
- Disabled drag and drop import using old Sketch importer.
- Make Fuse Studio free, with no license required
- Fixed issue where introducing an `ux:Property` would in some cases report `Member 'EmulatePropertyChanged' could not be found on object of type 'Uno.UX.PropertyObject'` in preview.
- Added menu option `Project/Sketch import` for editing list of sketch files to import Sketch symbols from. See notes on the new Sketch importer.
- Added a file dialog for browsing for FileSource of an Image control
- Fixed a bug that caused comments to be removed when dragging into an element
- If Fuse Studio is unable to open devices.json for editing, an error message is now displayed. Previously, this could cause Fuse Studio to crash.
- Sketch importer
  - Opacity on layers now translate to opacity on the resulting ux panel
  - Fixed a bug where shadows where flipped in the X-direction in UX compared to Sketch
  - Shadow mode set to `PerPixel` in UX to yield the expected result
  - Shadows on texts in Sketch now supported
  - Warn about not supporting inner shadows in UX
  - Fixed a bug where shapes split into multiple parts by scissoring where ignored
  - Set TextWrapping mode to Wrap on all texts in UX for better default behavior
  - Multiple text styles on one text object in Sketch now gives warning when generating UX

- You can now filter custom classes in components view
- Improved the performance of the log view
- Fixed suggestions for `ux:Class` and `ux:Property` in the UX code completer
- Fixed an issue where in certain situations starting local preview of a project that was already open in Fuse would open a new Fuse window, rather than just focusing the existing window
- Improved performance of tool, especially when working with larger projects
- Fixed an issue where double clicking the Fuse tray icon on Windows would show an error message "Unable to create Fuse document client. Please try again."
- Fixed an issue where the building indicator bar would not show when you opened a project
- Clicking the "Build failed" and "Auto reload failed" indicator bars now opens the log view
- Fixed an issue on macOS where `UnoHost` processes were left running if Fuse crashed
- Fuse now prints information about OpenGL version when local preview starts, and gives an error message if the version is lower than 2.1
- Added tool tips to several label-less inspector editors
- Fixed an issue where some users had to log in again every time Fuse started
- When starting Fuse 1.4 for the time, you will be logged out even if you were logged in with a previous version
- Fixed an issue on macOS High Sierra where `fuse install sublime-plugin` would not work
- Fixed an issue on macOS where we kept polling for the installation status of Sublime and the Sublime plugin even after closing the Setup Guide window. This would cause a Fuse icon to appear and disappear periodically in the Dock, stealing focus from other applications
- Fixed an issue that made Fuse crash on macOS when clicking a menu item, when running various software with access to control the computer (like Moom, EVE and possibly others).
- Fixed console output so errors are written to stderr and other output to stdout
- Fixed so the logger names in `fuse.log` correspond more closely to the source of the message
- Added new command `fuse unoconfig` which prints the current Uno configuration. `fuse preview` now also accepts a flag `--print-unoconfig` which will print the current Uno configuration before starting preview
- Removed Ant from `fuse install android`, as it is no longer used
- Added Restart to the Viewport menu
- Fixed an issue where `fuse install android` would fail on macOS with case sensitive file systems
- Show busy indicator as a notification bar if building or reifying takes more than 250ms

- Increased limit of max open files from 1024 to 9999, to avoid "Too many open files" error

- New experimental Sketch Importer available. The new Sketch importer will give you more control over your design by converting Sketch symbols to UX classes. See [user guide](https://www.fusetools.com/docs/assets/sketch-symbol-import) for how to include Sketch files to your project. The old Sketch importer still works as before.

## 1.3.0

#### Preview
- Refreshing previews of large apps on mobile devices is now much faster. 
- You can now *Restart* a preview from the Viewport menu in order to reset all the app state.

#### Create class from selection (Pro)
- You can now create an UX class from an instance element. Effectively this means you can create new components without switching out of Fuse Studio.

#### Inspector (Pro)
- Removed "current color" from the Windows color picker, since the new color is updated in real time rather than when the user clicks "ok"
- Removed `MaxLength` from the `Text` inspector. This attribute is deprecated, and the inspector for it has a bug
- Compact Mode is now always called "Compact Mode". Previously, it was referred to as "Compact View" in some places
- Gradient stops can now be added from the inspector
- Added PageControl and Page to the primitives bar
- Add ClientPanel and Button to the primitives bar

#### Android
- Installing Android SDKs (`fuse install android`) can now be done from the Tools menu 

#### Misc bug fixes
- Fix crash when attempting to remove root element from hierarchy
- Fix problem in log view with multiple log messages printed on one line
- Fix preview not appearing when Fuse is used by multiple user accounts on Mac
- Fix indentation when moving or adding elements spanning several lines
- Removing elements using the main menu no longer causes Fuse to crash
- Fixed an issue where `fuse install android` would fail on macOS with case sensitive file systems
- Fixed a (minor) memory leak

## 1.2.0

#### Using the tool while offline
- Fixed a bug that meant that Fuse was not usable while not connected to the Internet.
#### Preview key events
- Fixed a bug where AltGr keyboard presses inside a preview viewport would be sent to the main window which meant that it wasn't possible to type e.g. the '@' character on Nordic keyboards.
- Fixed UX completion for tags inside the App tag.

## 1.1.0

#### Build flags dialog
- Added a new dialog for adding build flags (like `-DDEBUG_V8`) from within Fuse Studio. It can be found in the Preview menu.

#### Inspector
- Replaced the TextTruncation editor for text controls with an editor for MaxLength.
- Fixed a bug where the value of certain editors would take on the value from a previously selected element when selected on macOS.

#### File watching
- Fixed a bug where Fuse sometimes failed to notice when a file in a project had been created.

#### Update notificaitons
- Fixed a bug where the change log would not show up in update notifications

## 1.0.2
Fixed `fuse install android` problem, where `cmake` couldn't be installed. 

## 1.0.1

### Fuselibs Updates

#### ColumnLayout
- Fixed an issue that would result in a broken layout if a `Sizing="Fill"` was used there wasn't enough space for one column.

#### Bug in Container
- Fixed bug in Container which caused crash when the container had no subtree nodes. This caused the Fuse.MaterialDesign community package to stop working.

#### Fuse.Controls.Video
- Fixed a bug where we would trigger errors on Android if a live-stream was seeked or paused.

#### Experimental.TextureLoader
- Fixed an issue when loading images bigger than the maximum texture-size. Instead of failing, the image gets down-scaled so it fits.

#### GraphicsView
- Fixed issue where apps would not redraw when returning to Foreground

#### ScrollView
- Fixed possible nullref in Scroller that could happen in certain cases while scrolling a ScrollView
- Fixed nullref in Scroll that could happen if there are any pending LostCapture callbacks after the Scroller is Unrooted

#### Fuse.Elements
- Fixed an issue where the rendering of one element could bleed into the rendering of another element under some very specific circumstances.

### Fuse Studio Updates

#### Command-line arguments
The `preview` command now (again) forwards defines (`-D` flags like `-DDEBUG_V8`) and verbosity flags (`-v`) given on the command-line to local preview.

#### MacOS window
The tool's window is now zoomed when the titlebar is double clicked.

#### Attributes section
The type of a property is now shown as the placeholder text of its editor in the attributes section of the inspector.

#### Project menu
- Fixed a bug where Fuse Studio would crash when opening files from this menu on some systems.

### Uno Updates

#### Fixed Android build error in NDK R15
- Fixed `use of undeclared identifier 'pthread_mutex_lock_timeout_np'` build error when using NDK R15, that was reported [here](https://www.fusetools.com/community/forums/bug_reports/android_buildpreview_failing_with_fuse_10).

## 1.0

### Highlights

- Fuse Studio released!
- Charting (Premium component)
- Xcode & Android Studio integration (Premium component)
- UX kit (Premium component)
- CameraView (Premium component)
- Dark Theme for the tool
- New warnings / problems tab to help you out with missing databindings (and much more!)
- And *finally* an option for clearing the log window

There's a lot going on in this release so make sure to read on...

### Fuse Studio and Premium components

This release introduces several *major* tools & features. Most notable here is the first version of the Fuse Studio visual tooling.

Fuse Studio, together with a set of feature packages that we call _Premium Components_, are available as part of our paid [Professional & Custom subscription plans](https://www.fusetools.com/plans). You can also get a free 30-day trial license [here](https://www.fusetools.com/trial). 
Once you have a license you just click "Log In" in the Fuse dashboard and enter your account details.


#### Fuse Studio key features
- A hierarchical overview of your app that makes layout and UX structuring even more intuitive.
- Basic UX markup editing by dragging & dropping default primitives and custom components. 
- Inspector for easy tweaking & discovery of element attributes - from colors & layout to data bindings.
- Multiple preview viewports. View different resolutions, pages and states side-by-side to see how your changes affect the *whole* app.

See the docs [here](https://www.fusetools.com/docs/fuse-studio/fuse-studio)

As you can probably imagine, this release is a Big Deal for us. That said, the most important thing is not what Fuse Studio does _today_ (although that's pretty cool) but how it will keep evolving with large & small features that'll make life easier for all of us when developing apps.

#### Xcode & Android Studio integration (Premium component)
Create custom components in Fuse and export them as native libraries for use in your existing native projects. Read more [here](https://www.fusetools.com/docs/xcode-and-android-studio-integration/tutorial) about how you can sprinkle Fuse awesomeness on your legacy code.

#### Charting (Premium component)
A powerful set of tools to easily create great-looking graphs and charts in your apps. [Docs](https://www.fusetools.com/docs/charting/charting)

#### Alive UX Kit (Premium component)
A collection of great-looking UI components for a wide range of use cases. Customise & evolve to your exact needs or use as-is. [Docs](https://www.fusetools.com/docs/alive/alive)

#### CameraView (Premium component)
A native view providing a customisable camera preview and photo functionality. (Yes, this means stickers-on-top-of-the-camera-stream!) [Docs](https://www.fusetools.com/docs/fuse/controls/cameraview)


### Other Updates

#### Dark theme
- Added a new dark theme for the Fuse application, which is now the default. It can be switched to the light theme in the "Window" menu. 

#### Log view
- Output in the log view is now grouped by the originating device
- Added a button to clear the log

#### Problems tab
- Added a new tab in the bottom panel called 'Problems', which shows temporary runtime errors or messages reported by apps.
- Shows Javascript syntax errors or exceptions, with a button to goto location
- Shows warnings when databinding sources aren't found

#### Simulator
- Fixed an issue resulting in the app being reified multiple times on rebuild
- Made the selection indicator visible for a second every time the selection changes even when the selection switch is off. This makes it easier to see what's selected when using the "Select element at caret" option.
- Fixed a problem that resulted in the simulator sometimes being reified with _old_ attribute values

#### Various tool fixes
- Fixed an issue where the log view tab would always overlap the app, and sometimes be misplaced in the UI
- Fixed spacing in the top bar. In particular, the compact view toggle was sometimes not displayed in compact view
- Fixed an issue where builds would sometimes fail due to connection errors to the package manager backend
- Fixed an issue where it was possible to resize panes such as the log view so large that it couldn't be resized back
- Improved startup time on macOS
- Fixed retrying after failed attempt to read locked file

#### Animation engine fixes
- Fixed a bug where elements with many children and some of them were rotated, the rotated elements would appear in the wrong location
