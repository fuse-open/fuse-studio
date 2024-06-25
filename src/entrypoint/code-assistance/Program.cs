using System;
using System.Linq;
using System.Threading;
using Outracks.Extensions;
using Outracks.IO;

namespace Outracks.Fuse.CodeAssistance
{
	class Program
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
		}

		static void Main(string[] argsArray)
		{
			var run = true;
			var args = argsArray.ToList();
			var fuse = FuseApi.Initialize("fuse-lang", args);
			var daemon = fuse.Connect("fuse-lang", TimeSpan.FromSeconds(30));
			daemon.ConnectionLost.Subscribe(d => run = false);

			var codeCompletionInstance = new CodeCompletionInstance(new ProjectDetector(new Shell()));
			daemon.Provide<GetCodeSuggestionsRequest, GetCodeSuggestionsResponse>(
				codeCompletionInstance.HandleGetCodeSuggestions);
			daemon.Provide<GotoDefinitionRequest, GotoDefinitionResponse>(codeCompletionInstance.HandleGotoDefinition);

			while (run)
			{
				Thread.Sleep(500);
			}
		}
	}
}
