using System;
using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks.Fusion
{
	public interface IDialog<in T>
	{
		void Focus();
		void Close(T result = default(T));
		void CloseWithError(Exception e);

		void CreateSingletonWindow(IObservable<bool> isVisible, Func<IDialog<object>, Window> window);

		Task<TU> ShowDialog<TU>(Func<IDialog<TU>, Window> window);
		
		Task<Optional<AbsoluteDirectoryPath>> BrowseForDirectory(AbsoluteDirectoryPath directory);
		Task<Optional<AbsoluteFilePath>> BrowseForFile(AbsoluteDirectoryPath directory, params FileFilter[] filters);

		//IControl BrowseForDirectoryControl(AbsoluteDirectoryPath directory);
	}
}