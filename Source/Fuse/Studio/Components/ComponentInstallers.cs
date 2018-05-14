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
				new AndroidInstaller(),
			    new AtomPlugin(),
				new VsCodePlugin(),
				new SublimePlugin(fuse.ModulesDir)
		    };
	    }
    }

}
