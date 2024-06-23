using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Xml;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator;

namespace Outracks.Fuse
{
	static class ExternalSelection
	{
		public static IDisposable UpdateProjectContext(IMessagingService daemon, IProject project, IScheduler scheduler = null)
		{
			scheduler = scheduler ?? Scheduler.Default;

			var includedFiles = project.Documents.Select(docs =>
				docs.Select(doc => doc.SimulatorIdPrefix).ToImmutableHashSet());

			return daemon
				.BroadcastedEvents<SelectionChanged>(false)

				// BEGIN HACK: bug in atom plugin is giving invalid selection when reloading sometimes
				.Where(s => s.Text.Length > 0 && s.CaretPosition.Character != 1)
				.Throttle(TimeSpan.FromMilliseconds(50), scheduler)
				.ObserveOn(Application.MainThread)
				// END HACK

				.WithLatestFromBuffered(includedFiles, (args, files) =>
					files.Contains(args.Path)
						? Optional.Some(new ObjectIdentifier(args.Path, TryGetElementIndex(args.Text, args.CaretPosition)))
						: Optional.None())
				.NotNone()

				.Select(elementId =>
					Observable.FromAsync(async () =>
					{
						var element = project.GetElement(elementId);
						await project.Context.Select(element);
					}))
				.Concat()
				.Subscribe();
		}

		internal static int TryGetElementIndex(string xml, Protocol.Messages.TextPosition caretPosition)
		{
			return TryGetElementIndex(xml, new Simulator.TextPosition(new Simulator.LineNumber(caretPosition.Line), new Simulator.CharacterNumber(caretPosition.Character)));
		}

		static int TryGetElementIndex(string xml, Simulator.TextPosition caretPosition)
		{
			try
			{
				var normalizedXml = xml.NormalizeLineEndings();
				var offset = caretPosition.ToOffset(normalizedXml);
				var parser = new AXmlParser();
				var tagsoup = parser.ParseTagSoup(new StringTextSource(normalizedXml));
				var elements = RemoveJavaScript(tagsoup.OfType<AXmlTag>());

				return elements
					.Where(e => !e.IsEndTag && !e.IsComment && e.ClosingBracket != "")
					.IndexOfFirst(obj => obj.StartOffset <= offset && offset < obj.EndOffset);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		static IEnumerable<AXmlTag> RemoveJavaScript(IEnumerable<AXmlTag> elements)
		{
			var skip = false;

			foreach (var element in elements)
			{
				if (element.Name == "JavaScript" && element.ClosingBracket == ">")
				{
					skip = !skip;
					yield return element;
				}
				else if (!skip)
				{
					yield return element;
				}
			}
		}
	}
}