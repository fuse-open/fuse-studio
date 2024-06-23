using System;

namespace Outracks
{
	public class Line
	{
		public readonly string Data;
		public readonly Interval<TextOffset> Span;

		public Line(string data, Interval<TextOffset> span)
		{
			Data = data;
			Span = span;
		}
	}


	public class TextScanner
	{
		readonly string _text;

		public TextOffset Position { get; private set; }

		public TextOffset End
		{
			get { return new TextOffset(_text.Length); }
		}

		public bool ReachedEnd { get { return Position >= End; } }

		public TextScanner(string text)
		{
			_text = text;
			Position = new TextOffset(0);
		}

		public Optional<TextOffset> ScanTo(string pattern)
		{
			var index = _text.IndexOf(pattern, Position, StringComparison.Ordinal);
			if (index == -1)
			{
				return Optional.None();
			}

			Position = new TextOffset(index + pattern.Length);
			return new TextOffset(index);
		}

		public bool IsNext(string pattern)
		{
			var index = PeekTo(pattern);
			return index == Position;
		}

		public Optional<TextOffset> ScanToAny(char []patterns)
		{
			var index = _text.IndexOfAny(patterns, Position);
			if (index == -1)
			{
				return Optional.None();
			}

			Position = new TextOffset(index + 1);
			return new TextOffset(index);
		}

		public Optional<TextOffset> PeekTo(string pattern)
		{
			var index = _text.IndexOf(pattern, Position, StringComparison.Ordinal);
			if (index == -1)
			{
				return Optional.None();
			}

			return new TextOffset(index);
		}

		public Optional<TextOffset> PeekToAnyPattern(string[] patterns)
		{
			var endIndex = new TextOffset(End + 1);
			var currentIndex = endIndex;
			foreach (var pattern in patterns)
			{
				var index = PeekTo(pattern);
				if (index.HasValue && index.Value < currentIndex)
				{
					currentIndex = index.Value;
				}
			}

			return currentIndex < endIndex ? Optional.Some(currentIndex) : Optional.None();
		}

		public string ReadTo(TextOffset end)
		{
			var text = _text.Substring(Position, end - Position);
			Position = new TextOffset(end);

			return text;
		}

		public string GetLineContaining(TextOffset offset)
		{
			var lastLineStart = 0;
			var currentLineStart = 0;
			for(var i = 0;i < _text.Length;++i)
			{
				if (_text[i] == '\n')
				{
					lastLineStart = currentLineStart;
					currentLineStart = i;
				}

				if (offset <= currentLineStart && offset >= lastLineStart)
				{
					return _text.Substring(lastLineStart, currentLineStart - lastLineStart);
				}
			}

			throw new ArgumentOutOfRangeException("offset");
		}
	}
}