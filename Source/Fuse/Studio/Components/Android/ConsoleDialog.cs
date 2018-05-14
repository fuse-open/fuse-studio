using System;
using System.IO;
using Outracks.AndroidManager;

namespace Outracks.Fuse
{
	class ConsoleDialog : IDialog
	{
		readonly ColoredTextWriter _textWriter;
		readonly TextReader _input;

		public ConsoleDialog(ColoredTextWriter textWriter, TextReader input)
		{
			_textWriter = textWriter;
			_input = input;
		}

		public TResult Start<TResult>(IDialogArguments<TResult> dialogArguments) 
			where TResult : IDialogResult
		{
			var yesNoResult = dialogArguments as DialogQuestion<DialogYesNoResult>;
			if (yesNoResult != null)
			{
				_textWriter.Write(yesNoResult.Question + " (y/n): ");
				var answer = _input.ReadLine();
				if(string.IsNullOrWhiteSpace(answer) == false 
					&& answer.Trim().ToLower() == "y")
					return (TResult)(IDialogResult)new DialogYesNoResult(true);
				else
					return (TResult)(IDialogResult)new DialogYesNoResult(false);
			}

			var pathResult = dialogArguments as DialogQuestion<DialogPathResult>;
			if (pathResult != null)
			{				
				_textWriter.Write(pathResult.Question);
				var rawData = _input.ReadLine();
				return (TResult)(IDialogResult) new DialogPathResult(rawData);
			}

			throw new ArgumentException("Unknown dialog argument", "dialogArguments");
		}
	}
}