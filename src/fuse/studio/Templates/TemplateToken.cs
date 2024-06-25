namespace Outracks.Fuse.Templates
{
	abstract class TemplateToken : IMatchTypes<TemplateVariable, TemplateText>
	{
	}

	sealed class TemplateVariable : TemplateToken
	{
		public readonly string Name;

		public TemplateVariable(string name)
		{
			Name = name;
		}
	}

	sealed class TemplateText : TemplateToken
	{
		readonly string _originalText;
		readonly TextOffset _offset;
		readonly int _length;

		public string Text
		{
			get { return _originalText.Substring(_offset, _length); }
		}

		public TemplateText(string originalText, TextOffset startOffset, TextOffset endOffset)
		{
			_originalText = originalText;
			_offset = startOffset;
			_length = endOffset - startOffset;
		}
	}
}