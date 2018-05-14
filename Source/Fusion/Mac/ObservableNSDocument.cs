using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Outracks.IO;

namespace Outracks.Fusion.OSX
{
	[Register("ObservableNSDocument")]
	public class ObservableNSDocument : NSDocument, IDocument
	{
		#region Static interface

		public static readonly ISubject<Window> DocumentOpened = new Subject<Window>();

		public static Task<IDocument> Open(AbsoluteFilePath path, bool giveFocus = false)
		{
			var tcs = new TaskCompletionSource<IDocument>();

			Fusion.Application.MainThread.Schedule(() =>
			{
				if (giveFocus)
					NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateAllWindows | NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

				var controller = (NSDocumentController)NSDocumentController.SharedDocumentController;
				controller.OpenDocument(path.ToNSUrl(), giveFocus,
					(document, wasAlreadyOpen, error) =>
					{
						if (error != null)
							tcs.TrySetException(new Exception(error.Description));
						else
							tcs.TrySetResult((ObservableNSDocument)document);	
					});
			});

			return tcs.Task;
		}

		#endregion

		readonly Subject<byte[]> _content = new Subject<byte[]>();
		public IObservable<byte[]> Content
		{
			get; private set;
		}

		readonly Subject<AbsoluteFilePath> _filePath = new Subject<AbsoluteFilePath>();
		public IObservable<Optional<AbsoluteFilePath>> FilePath
		{
			get; private set;
		}

		// Called from cocoa in response to open and new
		public ObservableNSDocument(IntPtr handle)
			: base(handle)
		{
			Console.WriteLine("new ObservableNSDocument()");

			var contentObs = _content.Replay(1);
			Content = contentObs;
			contentObs.Connect();

			var filePath = _filePath.Select(Optional.Some).Replay(1);
			FilePath = filePath;
			filePath.Connect();
		}

		// Called from cocoa during init
		NSUrl _fileUrl;
		public override NSUrl FileUrl
		{
			get { return _fileUrl; } // TODO: what i'm guessing is a bug in MonoMac causes the consistentncy check to fail on cocoa's own call to this method when we call base
			set
			{
				_filePath.OnNext(value.AbsoluteUrl.ToAbsoluteFilePath());
				base.FileUrl = _fileUrl = value;
			}
		}


		byte[] _dataWaitingForWrite = new byte[0];
		
		public void Write(byte[] content)
		{
			_dataWaitingForWrite = content;
			UpdateChangeCount(NSDocumentChangeType.Autosaved);
		}

		public IDialog<object> Window { get; private set; }

		// Called from cocoa during save (in response to UpdateChangeCount)
		public override NSData GetAsData(string documentType, out NSError outError)
		{
			Console.WriteLine("GetAsData(" + documentType + ")");
			outError = null;//NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return NSData.FromArray(_dataWaitingForWrite);
		}


		// Called from cocoa during open and reload
		public override bool ReadFromData(NSData data, string typeName, out NSError outError)
		{
			Console.WriteLine("ReadFromData(" + typeName + ")");
			outError = null;//NSError.FromDomain (NSError.OsStatusErrorDomain, -4);

			try
			{
				_content.OnNext(data.AsStream().ReadAllBytes());
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("ReadFromData failed: " + e);
				outError = new NSError(new NSString("com.fusetools.fuse"), 1, new NSMutableDictionary()
				{
					{ new NSString("ExceptionString"), new NSString(e.ToString()) }
				});
				return false;
			}
		}

		// Called from cocoa during init (iff opened with create window)
		public override void MakeWindowControllers()
		{			
			Console.WriteLine("Getting window");
			MacDialog<Unit>.ShowDocumentWindow(this, window =>
			{
				Window = window;
				var content = Fusion.Application.CreateDocumentWindow(this);
				Console.WriteLine("Got window");
				DocumentOpened.OnNext(content);
				Console.WriteLine("Will show!");
				return content;
			});
		}

		public override void WindowControllerDidLoadNib (NSWindowController windowController)
		{
			Console.WriteLine("WindowControllerDidLoadNib()");
			base.WindowControllerDidLoadNib (windowController);
			// Add code to here after the controller has loaded the document window
		}

		public override bool ValidateUserInterfaceItem(NSObject anItem)
		{
			Console.WriteLine("ValidateUserInterfaceItem()");
			return base.ValidateUserInterfaceItem(anItem);
		}

		
	}
}
