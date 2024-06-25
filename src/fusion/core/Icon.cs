using System;
using System.IO;
using System.Reflection;
using Outracks.IO;

namespace Outracks.Fusion
{
    public class Icon
    {
	    public static Icon FromFile(AbsoluteFilePath path)
	    {
		    return new Icon(() => File.OpenRead(path.NativePath));
	    }

	    public static Icon FromResource(string resourceName, Assembly assembly = null)
		{
			if (assembly == null)
				assembly = Assembly.GetCallingAssembly();

		    return new Icon(
			    () =>
			    {
					var stream = assembly.GetManifestResourceStream(resourceName);
					if (stream == null)
						throw new ArgumentException("Resource '"+resourceName+"' not found in assembly " + assembly.GetName().FullName);
					return stream;
			    });
	    }

	    readonly Func<Stream> _getStream;

	    public Icon(Func<Stream> getStream)
        {
	        _getStream = getStream;
        }

	    public Stream GetStream()
	    {
		    return _getStream();
	    }
    }
}