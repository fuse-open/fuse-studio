using System.Collections.Generic;

namespace Outracks.Fuse.Components
{
    public class ComponentInstallers
    {
	    public readonly List<ComponentInstaller> Components;

	    public ComponentInstallers(IFuse fuse)
	    {
		    Components = new List<ComponentInstaller>
		    {
				new AndroidBuildTools(fuse.ComponentsDir),
			    new AtomPlugin(),
				new VsCodeExtension(fuse.ComponentsDir),
#pragma warning disable CS0612 // Type or member is obsolete
				new VsCodePlugin(fuse.ComponentsDir),
#pragma warning restore CS0612 // Type or member is obsolete
				new SublimePlugin(fuse.ComponentsDir)
		    };
	    }
    }
}
