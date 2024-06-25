using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	static class LabelImplementation
	{
		public static void Initialize(Dispatcher dispatcher, IObservable<Ratio<Pixels, Points>> density)
		{
			Label.Implementation.Factory = (font, text, textAlignment, color, lineBreakMode) =>
			{
				var availableSize = new Size<IObservable<Points>>();
				var recalculateDesiredSize = new Subject<Unit>();
				var ctrl = Control.Create(self =>
				{
					var control = new TextBlock
					{
						TextWrapping = lineBreakMode == LineBreakMode.Wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
						TextTrimming = lineBreakMode == LineBreakMode.TruncateTail ? TextTrimming.WordEllipsis : TextTrimming.None,
						IsHitTestVisible = false
					};

					var actualWidth = self.NativeFrame.Width;
					availableSize = self.AvailableSize;

					var labelArgs = Observable.CombineLatest(text, font.Size, font.Bold, actualWidth,
						(val, size, bold, aw) => new
						{
							Text = val,
							Size = size,
							Bold = bold,
							ActualWidth = aw
						});

					self.BindNativeProperty(dispatcher, "labelArgs", labelArgs,
						args =>
						{
							control.Text = lineBreakMode == LineBreakMode.TruncateHead
								? TruncateHead(
									text: args.Text,
									widthConstraint: args.ActualWidth,
									measureStringSize: str => MeasureString(control, str, control.FontSize, control.FontWeight == FontWeights.ExtraBlack))
								: args.Text;
							control.FontSize = args.Size;
							control.FontWeight = args.Bold ? FontWeights.Bold : FontWeights.Normal;
							recalculateDesiredSize.OnNext(Unit.Default);
						});

					self.BindNativeProperty(
						dispatcher,
						"color",
						color,
						value => control.Foreground = new SolidColorBrush(value.ToColor()));

					self.BindNativeProperty(
						dispatcher,
						"textAlignment",
						textAlignment,
						value => control.TextAlignment = value.ToWpf());

					self.BindNativeDefaults(control, dispatcher);

					return control;
				});

				return ctrl
					.WithSize(
						Fusion.Application.MainThread
							.InvokeAsync(() => ctrl.NativeHandle)
							.ToObservable()
							.ObserveOn(Fusion.Application.MainThread)
							.Select(handle => MeasureString(dispatcher, (TextBlock)handle, recalculateDesiredSize, availableSize))
							.Switch()
							.Replay(1)
							.RefCount()
							.Transpose())
					.WithPadding(left: new Points(3), right: new Points(3));
			};

			Label.Implementation.Formatted =
				(textElements, font, textAlignment, color, lineBreakMode) =>
				Control.Create(self =>
						{
							var control = new TextBlock
							{
								TextWrapping = lineBreakMode == LineBreakMode.Wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
								TextTrimming = lineBreakMode == LineBreakMode.TruncateTail ? TextTrimming.WordEllipsis : TextTrimming.None,
							};

							self.BindNativeProperty(dispatcher, "font.Size", font.Size, value => control.FontSize = value);
							self.BindNativeProperty(
								dispatcher,
								"font.Bold",
								font.Bold,
								value => control.FontWeight = value ? FontWeights.Bold : FontWeights.Normal);

							self.BindNativeProperty(
								dispatcher,
								"color",
								color,
								value => control.Foreground = new SolidColorBrush(value.ToColor()));

							self.BindNativeProperty(
								dispatcher,
								"textAlignment",
								textAlignment,
								value => control.TextAlignment = value.ToWpf());

							self.BindNativeDefaults(control, dispatcher);

							self.BindNativeProperty(
								dispatcher,
								"text",
								textElements,
								elements =>
								{
									control.Inlines.Clear();
									foreach (var element in elements)
									{
										var inline = ToInline(element);
										if (element.ForegroundColor.HasValue)
										{
											inline.Foreground = new SolidColorBrush(element.ForegroundColor.Value.ToColor());
										}
										control.Inlines.Add(inline);
									}
								});

							return control;
						})
					.WithHeight(font.Size.Select(s => new Points(s * 1.25)))
					.WithWidth(70);
		}

		static Inline ToInline(TextPart part)
		{
			if (part is Url)
			{
				var l = (Url)part;
				var link = new Hyperlink(new Run(l.Text)) { NavigateUri = l.Uri };

				link.RequestNavigate += (sender, args) =>
				{
					Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri));
					args.Handled = true;
				};
				return link;
			}

			return new Run(part.Text);
		}

		static string TruncateHead(string text, Points widthConstraint, Func<string, Size<Points>> measureStringSize)
		{
			var textSize = measureStringSize(text);
			var ellipsisWidth = measureStringSize("...").Width;
			var desiredWidth = textSize.Width + ellipsisWidth;

			if (desiredWidth <= widthConstraint)
				return text; // Early out test
			if (ellipsisWidth > widthConstraint)
				return "";

			var diff = desiredWidth - widthConstraint;
			var words = Regex.Matches(text, @"\w(?<!\d)[\w'-]*");
			var currentReduction = new Points(0);
			var wordToBreakOn = new Optional<Match>();

			for (var i = 0; i < words.Count; ++i)
			{
				var word = words[i];

				var wordSize = measureStringSize(word.Value);
				currentReduction += wordSize.Width;
				if (currentReduction >= diff)
				{
					if (i + 1 < words.Count)
						wordToBreakOn = words[i + 1];

					break;
				}
			}

			return "..." + wordToBreakOn.Select(w => text.Substring(w.Index, text.Length - w.Index)).Or("");
		}

		static IObservable<Size<Points>> MeasureString(
			Dispatcher dispatcher,
			TextBlock prototype,
			IObservable<Unit> recalculateDesiredSize,
			Size<IObservable<Points>> availableSize)
		{
			var availS = prototype.TextWrapping != TextWrapping.NoWrap
				? availableSize.Transpose().StartWith(new Size<Points>(double.PositiveInfinity, double.PositiveInfinity))
				: Observable.Return(new Size<Points>(double.PositiveInfinity, double.PositiveInfinity));
			return availS.CombineLatest(recalculateDesiredSize.StartWith(Unit.Default), (a,d) => a)
				.Select(availSize =>
					dispatcher.InvokeAsync(() =>
					{
						var oldW = prototype.Width; var oldH = prototype.Height;
						prototype.Width = double.NaN; prototype.Height = double.NaN;
						prototype.Measure(availSize.Max(0,0).ToWpf());
						prototype.Width = oldW; prototype.Height = oldH;
						return prototype.DesiredSize.ToFusion();
					}))
				.Switch();
		}

		static Size<Points> MeasureString(
			TextBlock prototype,
			string text, double size, bool bold)
		{
			var formattedText = new FormattedText(
				text,
				CultureInfo.CurrentUICulture,
				FlowDirection.LeftToRight,
				new Typeface(
					prototype.FontFamily,
					prototype.FontStyle,
					bold ? FontWeights.ExtraBlack : FontWeights.Normal,
					prototype.FontStretch),
				size,
				Brushes.Black);

			return new Size<Points>(formattedText.Width, formattedText.Height);
		}
	}
}