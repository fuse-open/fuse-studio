## AutoReload
_AutoReload is a tool and library for iterating on Fusion UI by using JIT, so that changes can be reflected without having to rebuild._

### Usage

#### As standalone tool
_AutoReload_ can be used as a standalone executable, and is called `Fuse Sandbox.exe` on Windows and `Fuse Sandbox.app` on macOS. On windows the output directory is `$(ProjectDir)bin/Debug`, on macOS it's `$(ProjectDir)`. To open it on macOS, run `open ./Source/Fuse/Sandbox/Fuse\ Sandbox.app` from the repository root directory.

As a start, it's recommended to just put all your code in `AutoReloadContent.cs` (In the `Outracks.Fuse.Sandbox` project). Every time you save that file, the `Fusion-AutoReload` window updates to reflect your changes. It is also possible to split your code into multiple files, in which case you need to specify those in the argument list.

#### As a library

_AutoReload_ can also be linked with a project, and used by adding `.AutoReload()` to an `IControl` eg.

```csharp
static class MyControl
{
	public static IControl Create(MyData someData, MyDate otherData)
	{
		return Control.Empty
			.AutoReload(new object[] { someData, otherData });
	}
}
```

If the method your're using `AutoReload` in takes arguments (like `someData` and `otherData` above), you need to specify those in the `new object[] {}` argument to `AutoReload()` as shown above.

As you see, the `AutoReload` method supports forwarding of parameters, so that you can use real data in your control, while doing reloading.

Limitations with this approach are:
* The method containing `AutoReload` has to be a static method of a static class.
* Types from another file, referenced in the class or method, have to be public.

So basically you can have auto-reload support of existing controls, while your application is running. This gives you the ability to change the code, and see your changes immediately, without having to rebuild. 

#### Troubleshooting

##### Referencing the AutoReload project

###### Symptom
When you try to add `AutoReload` to something, it says `Cannot resolve symbol AutoReload`

###### Solution
- Make sure the project containing the file you want to change references the `AutoReload` project. For instance in Rider, press `Shift+Alt+l` to jump to the file in the solution explorer, find `References` right below the project name, right click, select "Add Reference", search and ad the `AutoReload` project.
- Also make sure you're `using` the correct namespace. Most IDEs will do this automatically, but make sure it says `using Outracks.Fusion.AutoReload;` near the top of the file you're adding `AutoReload()` in.
