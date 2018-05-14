using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Designer
{
	class DiagnosticHeuristicComparer : IEqualityComparer<Diagnostic>
	{
		public bool Equals(Diagnostic x, Diagnostic y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				return false;

			return x.Message == y.Message 
				&& x.SourceFile == y.SourceFile
				&& x.Type == y.Type
				&& x.Details == y.Details
				&& x.LineNumber == y.LineNumber
				&& x.ColumnNumber == y.ColumnNumber;
		}

		public int GetHashCode(Diagnostic obj)
		{
			unchecked
			{
				var hashCode = obj.Message != null ? obj.Message.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (obj.SourceFile != null ? obj.SourceFile.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Details != null ? obj.Details.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.Type.GetHashCode();
				hashCode = (hashCode * 397) ^ obj.LineNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ obj.ColumnNumber.GetHashCode();
				return hashCode;
			}
		}
	}

	public class ErrorView : IDisposable
	{
		readonly IObservable<AbsoluteFilePath> _projectFile;
		readonly IMessagingService _daemon;
		readonly BehaviorSubject<ImmutableList<Diagnostic>> _diagnostics = new BehaviorSubject<ImmutableList<Diagnostic>>(ImmutableList<Diagnostic>.Empty);
		readonly IDisposable _disposables;
		readonly IObservableList<BuildIssueDetected> _errorMessages;
		public readonly IObservable<bool> NotifyUser;

		public ErrorView(
			IObservable<IBinaryMessage> simulatorMessages, 
			IObservable<AbsoluteFilePath> projectFile, 
			IMessagingService daemon, 
			IObservable<string> serverClientRemoved)
		{
			_projectFile = projectFile;
			_daemon = daemon;
			var diagnosticsStream = simulatorMessages.TryParse(Diagnostic.MessageType, Diagnostic.ReadDataFrom);
			var dismissDiagnosticsStream = simulatorMessages.TryParse(DismissDiagnostic.MessageType, DismissDiagnostic.ReadDataFrom);


			var buildIssues = ErrorMessageListener.ListenForBuildErrors(
					simulatorMessages.TryParse(Started.MessageType, Started.ReadDataFrom),
					simulatorMessages.TryParse(BuildIssueDetected.MessageType, BuildIssueDetected.ReadDataFrom),
					out _errorMessages);

			NotifyUser = _diagnostics
				.Throttle(TimeSpan.FromMilliseconds(100))
				.DistinctUntilSetChanged()
				.Select(l => l.IsEmpty() == false);

			_disposables = Disposable.Combine(				
				diagnosticsStream.Subscribe(
					d => _diagnostics.OnNext(_diagnostics.Value.Add(d))), // Add diagnostics
				dismissDiagnosticsStream.Subscribe(
					d => _diagnostics.OnNext(_diagnostics.Value.RemoveAll(a => a.DiagnosticId == d.DiagnosticId && a.DeviceId == d.DeviceId))), // Remove diagnostics based on diagnostic id
				serverClientRemoved.Subscribe(
					clientRemoved => _diagnostics.OnNext(_diagnostics.Value.RemoveAll(a => a.DeviceId == clientRemoved))), // Remove diagnostics when clients disconnect
				buildIssues);
		}

		/*TODO: Currently we don't get the syntax errors from JS files going through ES6 transpiler. Not sure how to get access to them.*/
		public IControl Create()
		{
			var buildErrorItems =
				_errorMessages.Select(MapBuildIssue);

			var diagnosticErrorItems =
				_diagnostics.ObserveOn(Application.MainThread)
					.CombineLatest(
						_projectFile,
						(items, projFile) => items.Distinct(new DiagnosticHeuristicComparer()).Select(item => MapDiagnostic(item, projFile.ContainingDirectory)).ToImmutableList())
				.ToObservableList();

			var combinedErrorItems =
				buildErrorItems.Concat(diagnosticErrorItems);

			return
				combinedErrorItems
					.ToObservableImmutableList()
					.Throttle(TimeSpan.FromMilliseconds(100)) // To accommodate for when errors shortly disappear and then reappear
					.ObserveOn(Application.MainThread)
					.CachePerElement(CreateItemRow).StackFromTop()
				.WithPadding(new Thickness<Points>(10, 10))
				.MakeScrollable(darkTheme: Theme.IsDark);
		}

		IControl CreateItemRow(ErrorItem e)
		{
			var locateInEditor = Command.Create(_projectFile.Select(
				projFile => e.Source.Select(
					source =>
					{
						return (Action) (() =>
						{
							var request = new FocusEditorRequest()
							{
								Column = source.Column,
								Line = source.Line,
								File = source.File.NativePath,
								Project = projFile.NativePath
							};
							FocusEditorCommand.SendRequest(_daemon, request);
						});
					})));

			var tags =
				Layout.StackFromLeft(e.Tags.Select(
					tag => Layout.StackFromLeft(
						Label.Create(tag.Name, font: Theme.DefaultFontBold)
							.WithPadding(new Thickness<Points>(4, 2))
							.WithBackground(
								Shapes.Rectangle(fill: tag.Brush, cornerRadius: Observable.Return(new CornerRadius(10))))).WithPadding(right: new Points(4.0))));

			var lineInfoControl = _projectFile.Select(projFile =>  e.Source.Select(
				source =>
				{
					return Layout.StackFromLeft(
						Control.Empty.WithWidth(16),
						Layout.StackFromLeft(
							Label.Create("File: ", color: Theme.DescriptorText).CenterVertically(),
							Label.Create(
									source.File.RelativeTo(projFile.ContainingDirectory).NativeRelativePath,
									color: Theme.DefaultText,
									lineBreakMode: LineBreakMode.TruncateHead)
								.CenterVertically().WithWidth(200)),
						Layout.StackFromLeft(
							Label.Create("Line: ", color: Theme.DescriptorText).CenterVertically(),
							Label.Create(
								source.Line.ToString() + " : " + source.Column.ToString(),
								color: Theme.DefaultText).CenterVertically(),
							Button.Create(
								clicked: locateInEditor,
								content: states =>
									Label.Create("Locate", color: Theme.Link)).WithPadding(new Thickness<Points>(10, 0)).CenterVertically()),
						Control.Empty.WithWidth(8));
				}).Or(() => Control.Empty));

			return Layout.StackFromLeft(Layout.StackFromTop(
				Control.Empty.WithHeight(8),
				Label.Create(
					e.Title,
					color: Theme.DefaultText,
					//lineBreakMode: LineBreakMode.Wrap,
					font: Font.SystemDefault(Observable.Return(19.0))).CenterVertically(),
				Control.Empty.WithHeight(16),
				Layout.StackFromLeft(tags, lineInfoControl.Switch()),
				Control.Empty.WithHeight(8),
				Layout.StackFromLeft(
					Label.Create(e.Message, lineBreakMode: LineBreakMode.Wrap, color: Theme.DescriptorText)
						.WithPadding(new Thickness<Points>(10, 20))).WithBackground(
					Shapes.Rectangle(fill: Theme.FaintBackground, cornerRadius: Observable.Return(new CornerRadius(2))))));
		}

		ErrorItem MapDiagnostic(Diagnostic diagnostic, AbsoluteDirectoryPath root)
		{
			var sourceInfo =
				diagnostic.SourceFile.ToOptional()
					.Where(sourceFile => !string.IsNullOrWhiteSpace(sourceFile))
					.Select(
						sourceFile => new ErrorSourceInfo
						{
							Column = diagnostic.ColumnNumber,
							Line = diagnostic.LineNumber,
							File = FilePath.ParseAndMakeAbsolute(sourceFile, root)
						});

			return new ErrorItem
			{
				Message = diagnostic.Details,
				Source = sourceInfo,
				Tags = new[]
				{
					new ErrorTag { Brush = Theme.NotificationBarBackground, Name = "JS" },
					new ErrorTag { Brush = Theme.ReifyBarBackground, Name = "UX" }
				},
				Title = diagnostic.Message
			};
		}

		ErrorItem MapBuildIssue(BuildIssueDetected buildIssue)
		{
			var source = buildIssue.Source
				.Select(
					s => s.Location.Select(
						loc => new ErrorSourceInfo { Column = loc.Character, Line = loc.Line, File = AbsoluteFilePath.Parse(s.File) }))
				.SelectMany(x => x);

			return new ErrorItem
			{
				Message = buildIssue.Message,
				Source = source,
				Tags = new[] { new ErrorTag { Brush = Theme.ErrorColor, Name = "Build" } },
				Title =  buildIssue.Message.Contains(" Line") ? buildIssue.Message.BeforeFirst(" Line") : buildIssue.Message
			};
		}

		public void Dispose()
		{
			_disposables.Dispose();
			_diagnostics.Dispose();
		}

		static class ErrorMessageListener
		{
			public static IDisposable ListenForBuildErrors(IObservable<Started> buildStarted, IObservable<BuildIssueDetected> issuesDetected, out IObservableList<BuildIssueDetected> issues)
			{
				var errors = new ListBehaviorSubject<BuildIssueDetected>();
				issues = errors;

				return Disposable.Combine(
					buildStarted.Select(b => BinaryMessage.TryParse(b.Command, BuildProject.MessageType, BuildProject.ReadDataFrom))
						.NotNone()
						.Subscribe(_ => errors.OnClear()),

					buildStarted.Select(b => BinaryMessage.TryParse(b.Command, GenerateBytecode.MessageType, GenerateBytecode.ReadDataFrom))
						.NotNone()
						.Subscribe(_ => errors.OnClear()),

					issuesDetected.Subscribe(
						issue =>
						{
							errors.OnAdd(issue);
						}));
			}
		}


		class ErrorSourceInfo
		{
			public int Column { get; set; }
			public int Line { get; set; }
			public AbsoluteFilePath File { get; set; }

			public override bool Equals(object obj)
			{
				var info = obj as ErrorSourceInfo;
				return info != null &&
					   Column == info.Column &&
					   Line == info.Line &&
					   EqualityComparer<AbsoluteFilePath>.Default.Equals(File, info.File);
			}

			public override int GetHashCode()
			{
				var hashCode = 1944568825;
				hashCode = hashCode * -1521134295 + Column.GetHashCode();
				hashCode = hashCode * -1521134295 + Line.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<AbsoluteFilePath>.Default.GetHashCode(File);
				return hashCode;
			}
		}

		class ErrorTag
		{
			public Brush Brush { get; set; }
			public string Name { get; set; }

			public override bool Equals(object obj)
			{
				var tag = obj as ErrorTag;
				return tag != null &&
					   EqualityComparer<Brush>.Default.Equals(Brush, tag.Brush) &&
					   Name == tag.Name;
			}

			public override int GetHashCode()
			{
				var hashCode = -534460015;
				hashCode = hashCode * -1521134295 + EqualityComparer<Brush>.Default.GetHashCode(Brush);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
				return hashCode;
			}
		}

		class ErrorItem
		{
			public IReadOnlyList<ErrorTag> Tags { get; set; }
			public Optional<ErrorSourceInfo> Source { get; set; }
			public string Title { get; set; }
			public string Message { get; set; }

			public override bool Equals(object obj)
			{
				var item = obj as ErrorItem;
				return item != null &&
					   ((Tags != null && item.Tags != null && Tags.SequenceEqual(item.Tags)) || (Tags == null && item.Tags == null)) &&
					   Source.Equals(item.Source) &&
					   Title == item.Title &&
					   Message == item.Message;
			}

			public override int GetHashCode()
			{
				var hashCode = -307112755;
				if (Tags != null)
				{
					foreach (var tag in Tags)
						hashCode = hashCode * -1521134295 + EqualityComparer<ErrorTag>.Default.GetHashCode(tag);
				}
				hashCode = hashCode * -1521134295 + EqualityComparer<Optional<ErrorSourceInfo>>.Default.GetHashCode(Source);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
				return hashCode;
			}
		}
	}
}