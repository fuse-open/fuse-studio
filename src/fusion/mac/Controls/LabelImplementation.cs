using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AppKit;
using Foundation;

namespace Outracks.Fusion.Mac
{
	class LabelImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Label.Implementation.Factory = (font, text, textAlignment, color, lineBreakMode) =>
			{
				var desiredSize = new ReplaySubject<Size<Points>>(1);
				return Control.Create(self =>
				{
					var field = new NSTextField
					{
						// AcceptsMouseEvents = true,
						Cell = new NSTextFieldCell()
						{
							Font = NSFont.SystemFontOfSize(NSFont.SystemFontSize),
							TextColor = NSColor.FromCatalogName("System", "labelColor"),
							BackgroundColor = NSColor.FromCatalogName("System", "controlColor"),
							LineBreakMode = lineBreakMode.ToNSLineBreakMode()
						},
						WantsLayer = true
					};

					var availSize = field.Cell.LineBreakMode != NSLineBreakMode.Clipping
						? self.AvailableSize.Width.StartWith(99999)
						: Observable.Return<Points>(0);

					var labelArgs = Observable.CombineLatest(
						text, font.Size, font.Bold, availSize,
						(val, size, bold, maxWidth) => new
						{
							Text = val,
							Size = size,
							Bold = bold,
							MaxWidth = maxWidth
						});

					// Calulate initial desired size
					labelArgs
						.Take(1)
						.ObserveOn(Fusion.Application.MainThread)
						.Subscribe(args => { UpdateField(field, args.Text, args.Bold, args.Size, args.MaxWidth, desiredSize); });

					self.BindNativeDefaults(field, dispatcher);
					self.BindNativeProperty(
						dispatcher,
						"labelArgs",
						labelArgs,
						args => { UpdateField(field, args.Text, args.Bold, args.Size, args.MaxWidth, desiredSize); });

					self.BindNativeProperty(dispatcher, "textAlignment", textAlignment, (x) => field.Cell.Alignment = x.ToNSTextAlignment());
					self.BindNativeProperty(dispatcher, "color", color, (x) => field.TextColor = x.ToNSColor());

					return field;
				})
				.WithSize(desiredSize.Transpose().Round())
				.WithPadding(right: new Points(3)); // Make things symmetrical, NSTextField automatically adds padding on left side
			};


			Label.Implementation.Formatted = (textParts, font, textAlignment, color, lineBreakMode) =>
			Control.Create(self =>
			{
				var field = new NSTextField
				{
					Cell = new NSTextFieldCell()
					{
						Font = NSFont.SystemFontOfSize(NSFont.SystemFontSize),
						AllowsEditingTextAttributes = true,
						Selectable = true,
						FocusRingType = NSFocusRingType.None,
						Editable = false
					},
					WantsLayer = true
				};

				var attrStr = textParts
					.CombineLatest(
						color,
						font.Size,
						font.Bold,
						(t, c, fs, fb) => t.Select(part => ToNsMutabeAttributedString(part, c.ToNSColor(), fs, fb)))
					.Select(
						mutableParts =>
						{
							var tmp = new NSMutableAttributedString();
							foreach (var part in mutableParts)
							{
								tmp.Append(part);
							}
							return tmp;
						});

				self.BindNativeDefaults(field, dispatcher);
				self.BindNativeProperty(dispatcher, "fontSize", font.Size, size => field.Font = NSFont.SystemFontOfSize((float)size));
				self.BindNativeProperty(dispatcher, "fontBold", font.Bold, bold => field.Font = bold ? NSFont.BoldSystemFontOfSize(field.Font.PointSize) : NSFont.SystemFontOfSize(field.Font.PointSize));
				self.BindNativeProperty(dispatcher, "textAlignment", textAlignment, x => field.Alignment = x.ToNSTextAlignment());
				self.BindNativeProperty(dispatcher, "color", color, x => field.TextColor = x.ToNSColor());
				self.BindNativeProperty(dispatcher, "text", attrStr, x => field.AttributedStringValue = x);

				// TODO: wrap lines

				return field;
			})
			.WithHeight(font.Size.Select(s => new Points(s * 1.25)).Round());
		}

		static void UpdateField(NSTextField field, string text, bool fontBold, double fontSize, Points maxWidth, IObserver<Size<Points>> desiredSize)
		{
			field.Cell.Font = fontBold
				? NSFont.BoldSystemFontOfSize((float) fontSize)
				: NSFont.SystemFontOfSize((float)fontSize);
			field.StringValue = text;
			if (field.Cell.LineBreakMode != NSLineBreakMode.Clipping)
				field.PreferredMaxLayoutWidth = (nfloat)maxWidth.Value;
			desiredSize.OnNext(field.FittingSize.ToFusion());
			field.NeedsDisplay = true;
		}

		private static NSMutableAttributedString ToNsMutabeAttributedString(TextPart textPart, NSColor foreground, double fontSize, bool bold)
		{
			var attrStr = new NSMutableAttributedString(textPart.Text);
			var range = new NSRange(0, textPart.Text.Count());

			attrStr.BeginEditing();

			var url = textPart as Url;
			if (url != null)
			{
				attrStr.AddAttribute(NSStringAttributeKey.Link, new NSUrl(url.Uri.AbsoluteUri), range);
				attrStr.AddAttribute(NSStringAttributeKey.UnderlineStyle, new NSNumber(1), range);
				attrStr.AddAttribute(NSStringAttributeKey.Cursor, Cursor.Pointing.ToCocoa(), range);
			}

			attrStr.AddAttribute(
					NSStringAttributeKey.BackgroundColor,
					NSColor.Clear,
					range);

			attrStr.AddAttribute(
				NSStringAttributeKey.ForegroundColor,
				textPart.ForegroundColor.HasValue ? textPart.ForegroundColor.Value.ToNSColor() : foreground,
				range);

			// Fixes #2681 Setting the font size and weight for each part prevents the text formatting to change when
			// a link is clicked.
			attrStr.AddAttribute(
				NSStringAttributeKey.Font,
				bold ? NSFont.BoldSystemFontOfSize((float)fontSize) : NSFont.SystemFontOfSize((float)fontSize),
				range);

			attrStr.EndEditing();
			return attrStr;
		}

	}
}
