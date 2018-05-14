# Windows Installer

We use a toolset called Wix as a layer above the Microsofts installer library.

Wix lets you define different packages, define which actions to perform before or after each other and styling the user interface.

The installer is a separate VS solution that can be found in `...\Fuse\Installer\Windows`.

It is recommended to test the installer in a virtual machine when developing to avoid messing up your Windows Registry.

## Building the Installer

1. Download and install [Wix](http://wixtoolset.org/). We are currently on version 3.9.
1. Build Fuse using the pack script (?)
1. Copy the content into a folder `Fuse\Installer\Windows\Source\Fuse\`
2. Run one of the following
  1. `BuildDebugInstaller.bat`
  1. `BuildFinalInstaller.bat`
  1. `BuildReleaseInstaller.bat`

The resulting installer can then be found a corresponding subfolder in `BuildOutput`.

## Structure of the Windows installer solution

### Actions
### Bootstrapper
### Gui
### Product

### Setting up the bundle

### The UI

Because the standard Wix UI looks like something from the 90's we have decided to use a customized UI.


## Testing

The safest way to test the installer is on a virtual machine since we, among other things are writing to the Windows Registry. It also gives you the opportunity to test the installer on several different platforms and conditions. A pro tip would be to make a snapshot of the VM before testing the installer so that it can easily be rolled back to the condition it was in before the install.
