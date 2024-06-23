using System;

namespace Outracks.CodeCompletion
{
	public enum SuggestItemType
	{
		Keyword,
		Namespace,
		Class,
		Struct,
		Interface,
		Delegate,
		GenericParameterType,
		Enum,
		EnumValue,
		Constant,
		Field,
		Variable,
		MethodArgument,
		Method,
		Property,
		Event,
		MetaProperty,
		Block,
		BlockFactory,
		Importer,
		Directory,
		File,
		TypeAlias,
		Error,
        Constructor //Need some way to differentiate between methods and constructors..
	}

	public enum SuggestItemPriority
	{
		Normal,
		High
	}

	public interface IAutoCompleteCodeEditor
	{
		int GetCaretOffset();
		void InsertText(int offset, string text);
		void SetCarretPos(int offset);
		void RequestIntelliPrompt();
	}

    public struct MethodArgument
    {
        public string Name;
        public string ArgType;
        public bool IsOut;
        public MethodArgument(string name, string argType, bool isOut = false)
        {
            Name = name;
            ArgType = argType;
            IsOut = isOut;
        }
    }

	public class SuggestItem
	{
		public string Text { get; private set; }
		public SuggestItemType Type { get; private set; }
		public Func<string> HtmlDescriptionProvider { get; private set; }
		public Func<string> AutoCompletePreText { get; private set; }
        public Func<string> AutoCompletePostText { get; private set; }
        public Func<string> AutoCompleteDescriptionText { get; private set; } //Verbose description text
        public string[] AutoCompleteAccessModifiers { get; private set; }
        public string[] AutoCompleteFieldModifiers { get; private set; }
        public MethodArgument[] AutoCompleteMethodArguments { get; private set; }
		public Action<IAutoCompleteCodeEditor> CommitAction { get; private set; }
		public SuggestItemPriority Priority { get; private set; }

		public SuggestItem(
			string text,
			Func<string> htmlDescriptionProvider,
			SuggestItemType type,
			Func<string> autoCompletePreText = null,
            Func<string> autoCompletePostText = null,
            Func<string> autoCompleteDescriptionText = null,
            string[] autoCompleteAccessModifiers = null,
            string[] autoCompleteFieldModifiers = null,
            MethodArgument[] arguments = null,
			Action<IAutoCompleteCodeEditor> commitAction = null,
			SuggestItemPriority priority = SuggestItemPriority.Normal)
		{
			this.Text = text;
			this.HtmlDescriptionProvider = htmlDescriptionProvider;
			this.Type = type;
			this.AutoCompletePreText = autoCompletePreText;
            this.AutoCompletePostText = autoCompletePostText;
            this.AutoCompleteDescriptionText = autoCompleteDescriptionText;
            this.AutoCompleteAccessModifiers = autoCompleteAccessModifiers;
            this.AutoCompleteFieldModifiers = autoCompleteFieldModifiers;
            this.AutoCompleteMethodArguments = arguments;
			this.CommitAction = commitAction;
			this.Priority = priority;
		}

		public override string ToString()
		{
			return Text + " (" + Type.ToString() + ")";
		}
    }
}
