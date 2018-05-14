using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using AppKit;
using Foundation;

namespace Outracks.Fusion.OSX
{
	class LogViewImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			LogView.Implementation.Factory = (stream, color, clear, darktheme) =>
				Control.Create(self =>
					{
						var textView = new NSTextView()
						{
							Editable = false,
							DrawsBackground = false,
							VerticallyResizable = true,
							AutoresizingMask = NSViewResizingMask.WidthSizable,
							TextContainer =
							{
								WidthTracksTextView = true
							}
						};

						var ctrl = new NSScrollView()
						{
							DocumentView = textView,
							DrawsBackground = false,
							HasVerticalScroller = true,
							AutohidesScrollers = true
						};

						self.BindNativeProperty(dispatcher, "darktheme", darktheme, dark => ctrl.ScrollerKnobStyle = dark ? NSScrollerKnobStyle.Light : NSScrollerKnobStyle.Dark);

						self.BindNativeDefaults(ctrl, dispatcher);

						self.BindNativeProperty(dispatcher, "color", color,
							value =>
							{
								var area = new NSRange(0, textView.TextStorage.Length);
								textView.TextStorage.BeginEditing();
								textView.TextStorage.RemoveAttribute(NSStringAttributeKey.ForegroundColor, area);
								textView.TextStorage.AddAttribute(NSStringAttributeKey.ForegroundColor, value.ToNSColor(), area);
								textView.TextStorage.EndEditing();
							});

						clear.ObserveOn(Fusion.Application.MainThread)
							.Subscribe(_ =>
							{
								textView.TextStorage.MutableString.SetString(new NSString());
							});

						stream.Buffer(TimeSpan.FromSeconds(1.0 / 30.0))
							.Where(c => c.Count > 0)
							.WithLatestFromBuffered(color, (msgsToAdd, col) => new { MessagesToAdd = msgsToAdd, Color = col })
							.ObserveOn(Fusion.Application.MainThread)
							.Subscribe(data =>
							{
								var scrollToBottom = ctrl.DocumentVisibleRect.Y + ctrl.DocumentVisibleRect.Height + 10 >= textView.Frame.Height;

								textView.TextStorage.BeginEditing();
								foreach (var msg in data.MessagesToAdd)
									textView.TextStorage.MutableString.Append(new NSString(msg));
								textView.TextStorage.AddAttribute(NSStringAttributeKey.ForegroundColor, data.Color.ToNSColor(), textView.TextStorage.EditedRange);
								textView.TextStorage.EndEditing();

								if (scrollToBottom)
								{
									textView.ScrollRangeToVisible(new NSRange(textView.TextStorage.Length, 1));
								}
							});
						
						return ctrl;
					});

		}
	}
}
