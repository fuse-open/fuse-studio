using System;
using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks.Fuse.Refactoring
{
	public interface IClassExtractor
	{
		Task ExtractClass(IElement element, string name, Optional<RelativeFilePath> fileName);
		IObservable<string> LogMessages { get; }
	}
}