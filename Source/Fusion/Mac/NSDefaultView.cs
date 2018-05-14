using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AppKit;
using Foundation;
using Outracks.IO;
using System.IO;

using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Outracks.Fusion.OSX
{
	interface IObservableResponder
	{
		IObservable<object> GotFocus { get; }
		IObservable<object> LostFocus { get; } 
		
		IObservable<NSEvent> KeyDown { get; }
		IObservable<NSEvent> KeyUp { get; }

		Action<NSView> ResetCursorRectsHandler { get; set; }
	}

	class NSFlippedClipView : NSClipView
	{
		public override bool IsFlipped
		{
			get { return true; }
		}
		public override bool IsOpaque
		{
			get { return true; }
		}

		public NSFlippedClipView()
		{
			base.AutoresizesSubviews = false;
		}

		public NSFlippedClipView(IntPtr handle) : base(handle)
		{
			base.AutoresizesSubviews = false;
		}
	}


	class NSDefaultView : NSView, IObservableResponder
	{
		public override bool AcceptsFirstResponder()
		{
			return _keyDown.HasObservers || _keyUp.HasObservers;
		}

		public override bool IsOpaque
		{
			get { return false; }
		}

		public override bool MouseDownCanMoveWindow
		{
			get { return true; }
		}

		public NSDefaultView()
		{
			base.AutoresizesSubviews = false;
		}

		public NSDefaultView(IntPtr handle) : base(handle)
		{			
		}

		public Action<NSView> ResetCursorRectsHandler { get; set; }

		public override void ResetCursorRects()
		{
			if (ResetCursorRectsHandler != null)
				ResetCursorRectsHandler(this);
		}


		readonly Subject<object> _gotFocus = new Subject<object>();
		readonly Subject<object> _lostFocus = new Subject<object>();

		IObservable<object> IObservableResponder.GotFocus { get { return _gotFocus; } }
		IObservable<object> IObservableResponder.LostFocus { get { return _lostFocus; } }

		public override bool BecomeFirstResponder()
		{
			_gotFocus.OnNext(new object());
			return base.BecomeFirstResponder();
		}

		public override bool ResignFirstResponder()
		{
			_lostFocus.OnNext(new object());
			return base.ResignFirstResponder();
		}


		readonly Subject<NSEvent> _keyDown = new Subject<NSEvent>();
		readonly Subject<NSEvent> _keyUp = new Subject<NSEvent>();

		IObservable<NSEvent> IObservableResponder.KeyDown { get { return _keyDown; } }
		IObservable<NSEvent> IObservableResponder.KeyUp { get { return _keyUp; } }

		public override void KeyDown(NSEvent theEvent)
		{
			_keyDown.OnNext(theEvent);
			base.KeyDown(theEvent);
		}

		public override void KeyUp(NSEvent theEvent)
		{
			_keyUp.OnNext(theEvent);
			base.KeyUp(theEvent);
		}

		public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
		{
			if (AcceptDragSource(sender))
				return NSDragOperation.Generic;
			
			return NSDragOperation.None;
		}


		DropOperation _dropOperation = null;
		AbsoluteFilePath _droppedFile;

		public void AddDropOperation(DropOperation op)
		{
			_dropOperation = op;
		}

		bool AcceptDragSource(NSDraggingInfo sender)
		{
			if (_dropOperation == null) return false;
			try
			{
				// Check correct file type
				var pasteboard = sender.DraggingPasteboard;

				if (!pasteboard.Types.Contains(NSPasteboard.NSFilenamesType))
					return false;

				var data = pasteboard.GetPropertyListForType(NSPasteboard.NSFilenamesType) as NSArray;
				if (data == null || data.Count < 1)
					return false;

				var pathString = NSString.FromHandle(data.ValueAt(0));

				_droppedFile = AbsoluteFilePath.Parse(pathString);
				if (!File.Exists(_droppedFile.NativePath))
				{
					_dropOperation.OnError.OnNext("Dragged file not found: " + _droppedFile.NativePath);
					return false;
				}

				return _dropOperation.CanDrop(_droppedFile);
			}
			catch (Exception e)
			{
				_dropOperation.OnError.OnNext(e.Message);
			}
			return false;
		}


		public override bool PerformDragOperation(NSDraggingInfo sender)
		{

			if (_dropOperation == null) return false;
			try
			{
				_dropOperation.Drop(_droppedFile);
				return base.PerformDragOperation(sender);
			}
			catch (Exception e)
			{
				_dropOperation.OnError.OnNext("Drop operation failed: " + e.Message);
			}
			return false;
		}


		public override void ConcludeDragOperation(NSDraggingInfo sender)
		{
			_droppedFile = null;
		}


	}
}
