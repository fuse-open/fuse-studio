using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public class TextPart
	{
		
		public string Text { get; set; }
		// font
		// color
		public Optional<Color> ForegroundColor { get; set; }
	}
	
	public class Url : TextPart
	{
		public Uri Uri { get; set; }
	}

	public static class AttributedText
	{
		public static IList<TextPart> Parts()
		{
			return new List<TextPart>();
		}

		public static IList<TextPart> Text(this IList<TextPart> self, string element, Color color = null)
		{
			self.Add(new TextPart
			{
				Text = element,
			    ForegroundColor = color ?? Optional.None<Color>()
			});
			return self;
		}

		public static IList<TextPart> Link(this IList<TextPart> self, string name, string url, Color color = null)
		{
			self.Add(new Url
			{
				Text = name,
			    ForegroundColor = color ?? Optional.None<Color>(),
				Uri = new Uri(url)
			});
			return self;
		}
	}

	public enum LineBreakMode
	{
		/// <summary>
		/// Lines are simply not drawn past the edge of the text container.
		/// </summary>
		Clip,
		
		/// <summary>
		/// Wrapping occurs at word boundaries, unless the word itself doesn�t fit on a single line.
		/// </summary> 
		Wrap,
		
		/// <summary> 
		/// The line is displayed so that the end fits in the container and the missing text at the beginning of the line is indicated by an ellipsis glyph. 
		/// </summary> 
		TruncateHead,

		///<summary>
		/// The line is displayed so that the beginning fits in the container and the missing text at the end of the line is indicated by an ellipsis glyph.
		/// </summary> 
		TruncateTail 
	}

	public static class Label 
	{
		public static IControl Create(
			Text text = default(Text), 
			Font font = null, 
			TextAlignment textAlignment = TextAlignment.Left,
			Brush color = default(Brush),
			LineBreakMode lineBreakMode = LineBreakMode.Clip)
		{
			return Implementation.Factory(
				font ?? Font.Default,
				text,
				Observable.Return(textAlignment),
				color | new Color(1, 1, 1, 1),
				lineBreakMode);
		}

		public static class Implementation
		{
			public static Func<Font, Text, IObservable<TextAlignment>, Brush, LineBreakMode, IControl> Factory;
			public static Func<IObservable<IEnumerable<TextPart>>, Font, IObservable<TextAlignment>, Brush, LineBreakMode, IControl> Formatted;
		}

		public static IControl FormattedText(
			IObservable<IEnumerable<TextPart>> textParts,
			Font font = null,
			TextAlignment textAlignment = TextAlignment.Left,
			Brush color = default(Brush),
			LineBreakMode lineBreakMode = LineBreakMode.Clip)
		{
			return Implementation.Formatted(
				textParts, 
				font ?? Font.Default, 
				Observable.Return(textAlignment), 
				color | Color.FromBytes(255, 0, 0), 
				lineBreakMode);
		}
	}
}