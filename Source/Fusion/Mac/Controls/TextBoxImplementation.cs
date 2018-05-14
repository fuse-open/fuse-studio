using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Outracks.Fusion.OSX
{
	static class TextBoxImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			TextBox.Implementation.Factory = (value, isMultiLine, doWrap, onFocused, foregroundColor) =>
			{
				value = value.PreventFeedback();

				return Control
					.Create(self =>
						{
							var ctrl = new NSCustomCaretColorTextField()
							{
								Editable = true,
								Bordered = false,
								Cell = doWrap
									?  new NSTextFieldCell()
										{
											Font = NSFont.SystemFontOfSize(11),
											Wraps = doWrap,
											Editable = true,
										}
									: new VerticallyCenteredTextFieldCell()
										{
											Font = NSFont.SystemFontOfSize(11),
											Wraps = doWrap,
											Editable = true,
										},
								FocusRingType = NSFocusRingType.None
							};

							onFocused.Do(handler =>
								((IObservableResponder)ctrl).GotFocus
									.WithLatestFromBuffered(handler.Execute.ConnectWhile(self.IsRooted), (_, h) => h)
									.Subscribe(c => c()));

							self.BindNativeDefaults(ctrl, dispatcher);
							self.BindNativeProperty(dispatcher, "Enabled", value.IsReadOnly, isReadOnly => ctrl.Enabled = !isReadOnly);

							ctrl.BindStringValue(value, self.IsRooted);

							self.BindNativeProperty(dispatcher, "foreground", foregroundColor,
								fg =>
								{
									ctrl.TextColor = ctrl.CaretColor = fg.ToNSColor();
								});
			
							return ctrl;
						})
					.WithHeight(20);
			};
		}

		static void BindStringValue(
			this NSCustomCaretColorTextField control,
			IProperty<string> value,
			IObservable<bool> isRooted)
		{
			var setByUser = false;
			var hasUnsavedChanges = false;

			value = value
				.ConnectWhile(isRooted)
				.DistinctUntilChangedOrSet();

			DataBinding
				.ObservableFromNativeEvent<EventArgs>(control, "Changed")
				.Subscribe(_ =>
				{
					if (!setByUser)
						return;

					hasUnsavedChanges = true;
					value.Write(control.StringValue, save: false);
				});

			value.Subscribe(v => 
				Fusion.Application.MainThread.Schedule(() =>
				{
					setByUser = false;
					try
					{
						control.StringValue = v;
					}
					finally
					{
						setByUser = true;
					}
				}));

			((IObservableResponder)control).LostFocus.Subscribe(_ =>
			{
				if (!hasUnsavedChanges)
					return;

				value.Write(control.StringValue, save: true);
				hasUnsavedChanges = false;
			});
		}
	}

	class NSCustomCaretColorTextField : NSMouseTrackingTextField
	{
		public NSColor CaretColor { get; set; }

		public NSCustomCaretColorTextField()
		{			
		}

		public NSCustomCaretColorTextField(IntPtr handle) : base(handle)
		{			
		}

		public override void MouseEntered(NSEvent theEvent)
		{
			base.MouseEntered(theEvent);

			SetNativeCaretColor();
		}

		public override bool BecomeFirstResponder()
		{
			var success = base.BecomeFirstResponder();

			if (success)
				SetNativeCaretColor();

			return success;
		}

		void SetNativeCaretColor()
		{
			var textView = (NSTextView)Window.FieldEditor(true, this);
			textView.InsertionPointColor = CaretColor;
		}
	}

	class NSMouseTrackingTextField : NSTextField, IObservableResponder
	{
		readonly NSTrackingArea _trackingArea;

		public override bool AcceptsFirstResponder()
		{
			return true;
		}

		public NSMouseTrackingTextField(IntPtr handle) : base(handle)
		{
		}

		public NSMouseTrackingTextField()
		{
			_trackingArea = new NSTrackingArea(
				new CGRect(),
				NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.MouseEnteredAndExited,
				this,
				null);
			AddTrackingArea(_trackingArea);
		}

		public Action<NSView> ResetCursorRectsHandler { get; set; }

		public override void ResetCursorRects()
		{
			base.ResetCursorRects();
			if (ResetCursorRectsHandler != null)
				ResetCursorRectsHandler(this);
		}

		readonly Subject<object> _gotFocus = new Subject<object>();
		readonly Subject<object> _lostFocus = new Subject<object>();

		IObservable<object> IObservableResponder.GotFocus { get { return _gotFocus; } }
		IObservable<object> IObservableResponder.LostFocus { get { return _lostFocus; } }

		bool _hasFocus;
		NSObject _notificationToken;

		public override void ViewWillMoveToSuperview(NSView newSuperview)
		{
			if (newSuperview != null)
			{
				_notificationToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString(CustomWindowNotifications.WindowMakeFirstResponder), notification =>
				{
					var view = notification.Object as NSView;

					if (view == null) return;
					view = view.Superview;

					if (view == null) return;
					view = view.Superview;

					if (view == this && !_hasFocus)
					{
						_hasFocus = true;
						_gotFocus.OnNext(new object());
					}
				});
			}
			else
			{
				_notificationToken.Dispose();
			}
		}

		public override void DidEndEditing(NSNotification notification)
		{
			base.DidEndEditing(notification);

			if (_hasFocus)
			{
				_hasFocus = false;
				_lostFocus.OnNext(new object());
			}
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
	}

	class VerticallyCenteredTextFieldCell : NSTextFieldCell
	{
		bool _isEditing;
		bool _isSelecting;

		public VerticallyCenteredTextFieldCell()
		{
		}

		public VerticallyCenteredTextFieldCell(IntPtr handle) : base(handle)
		{
		}

		public override CGRect DrawingRectForBounds(CGRect theRect)
		{
			var baseRect = base.DrawingRectForBounds(theRect);

			if (_isEditing == false && _isSelecting == false)
			{
				var textSize = CellSizeForBounds(theRect);

				if (textSize.Height < baseRect.Height)
					return new CGRect(
						baseRect.X, baseRect.Y + (theRect.Height - textSize.Height) / 2,
						baseRect.Width, textSize.Height);
			}

			return baseRect;
		}

		public override void SelectWithFrame(CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, nint selStart, nint selLength)
		{
			var actualRect = DrawingRectForBounds(aRect);
			_isSelecting = true;
			base.SelectWithFrame(actualRect, inView, editor, delegateObject, selStart, selLength);
			_isSelecting = false;
		}

		public override void EditWithFrame(CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, NSEvent theEvent)
		{
			var actualRect = DrawingRectForBounds(aRect);
			_isEditing = true;
			base.EditWithFrame(actualRect, inView, editor, delegateObject, theEvent);
			_isEditing = false;
		}
	}
}
