using System;
using System.Reactive;
using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks.Fusion
{
	public interface IDocument
	{
		IObservable<Optional<AbsoluteFilePath>> FilePath { get; } 
		
		//IObservable<double> Progress { get; }

		IObservable<byte[]> Content { get; }
		
		void Write(byte[] content);

		IDialog<object> Window { get;  } 
		//void Rename(FileName name);
		//void Move(DocumentLocation location);
		//void Delete();

		//void Bookmark();	
	}
	
	/*
	class Bookmark
	{
		IObservable<string> DisplayName { get; }
		IObservable<string> DisplayLocation { get; }

		IDocument Open();

		void Remove();
	}
	*/
}
