using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Refactoring
{
	public class ExtractClassButtonViewModel : IExtractClassButtonViewModel
	{
		readonly Command _command;
		readonly ISubject<bool> _hovering;
		readonly IObservable<bool> _highlightSelectedElement;
		readonly Subject<string> _log = new Subject<string>();
		readonly IList<string> _invalidUxClassNames = new List<string>()
		{
			"JavaScript",
			"Include"
		};

		public Command Command
		{
			get { return _command; }
		}

		public IObservable<bool> HighlightSelectedElement
		{
			get { return _highlightSelectedElement; }
		}

		public void HoverEnter()
		{
			_hovering.OnNext(true);
		}

		public void HoverExit()
		{
			_hovering.OnNext(false);
		}

		public IObservable<string> Log
		{
			get { return _log; }
		}

		public ExtractClassButtonViewModel(
			IProject project, 
			IContext context, 
			Action<IExtractClassViewModel> openDialogAction, 
			IFileSystem fileSystem,
			IClassExtractor classExtractor)
		{
			_hovering = new BehaviorSubject<bool>(false);

			var allClassNames = project.Classes.Select(
				elements =>
				{
					var elementList = elements as IList<IElement> ?? elements.ToList();
					if (!elementList.Any())
					{
						return Observable.Return(new List<Optional<string>>());
					}
					return elementList.Select(el => el.UxClass()).CombineLatest();
				}).Switch().Select(x => new HashSet<string>(x.NotNone())).Replay(1).RefCount();
			
			var canExtractSelectedElement = context.CurrentSelection.IsEmpty.IsFalse()
				.And(context.CurrentSelection.Parent.IsEmpty.IsFalse()) // Not the App(root) element
				// We can't extract an element which is already a class
				.And(context.CurrentSelection.UxClass().Is(Optional.None()))
				// Extracting classes listed in _invalidUxClassNames creates invalid UX
				.And(context.CurrentSelection.Name.Select(name => !_invalidUxClassNames.Contains(name)))
				.Replay(1)
				.RefCount();

			_highlightSelectedElement = _hovering.And(canExtractSelectedElement);

			_command = Command.Create(
				canExtractSelectedElement,
				context.CurrentSelection.Name.CombineLatest(
					allClassNames,
					(selectedElementName, classNames) =>
						(Action) (() =>
						{
							openDialogAction(
								 new ExtractClassViewModel(
								context: context,
								suggestedName: GetClassNameSuggestion(selectedElementName, classNames),
								allClassNames: allClassNames,
								classExtractor: classExtractor,
								fileSystem: fileSystem,
								project: project));
						})));
		}

		static string GetClassNameSuggestion(string selectedElementName, HashSet<string> classNames)
		{
			var primarySuggestion = "My" + selectedElementName;
			var suggestion = primarySuggestion;
			var suggestionCounter = 2;
			while (classNames.Contains(suggestion))
			{
				suggestion = primarySuggestion + suggestionCounter;
				suggestionCounter++;
			}
			return suggestion;
		}
	}
}