using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Refactoring
{
	public class ExtractClassViewModel : IExtractClassViewModel
	{
		// Seems like uno identifiers can only contain ASCII characters, is that correct?
		static readonly Regex ClassNameValidationRegexp = new Regex("^[A-Za-z_][A-Za-z0-9_]*(\\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.Compiled);

		readonly Command _createCommand;
		readonly IProperty<string> _className;
		readonly IProperty<string> _newFileName;
		readonly IProperty<bool> _createInNewFile;
		readonly IObservable<string> _userInfo;


		public ExtractClassViewModel(
			IContext context,
			string suggestedName,
			IObservable<HashSet<string>> allClassNames,
			IClassExtractor classExtractor,
			IFileSystem fileSystem,
			IProject project)
		{
			_className = Property.Create(suggestedName);
			_createInNewFile = Property.Create(false);
			var newFileNameSubject = new BehaviorSubject<string>("");
			var hasEditedFileName = false;

			_newFileName = newFileNameSubject.AsProperty(
				isReadOnly:
					_createInNewFile.Select(check => check == false),
				write: (name, save) =>
				{
					hasEditedFileName = true;
					newFileNameSubject.OnNext(name);
				});

			_className.Subscribe(
				name
				=>
				{
					if (!hasEditedFileName)
					{
						var filename = name.Replace(".", Path.DirectorySeparatorChar.ToString()) + ".ux";
						newFileNameSubject.OnNext(filename);
					}
				});

			var action = _className.CombineLatest(
					allClassNames,
					_createInNewFile,
					_newFileName,
					project.RootDirectory,
					(name, allClasses, toNewFile, fileName, projectPath) =>
					{
						// Don't care about whitespace at beginning or end
						name = name.Trim();

						if (name.IsNullOrEmpty())
							return new ValidatedAction("");

						if (name.Any(char.IsWhiteSpace))
							return new ValidatedAction("Class name can't contain whitespaces");

						if (!ClassNameValidationRegexp.IsMatch(name))
							return new ValidatedAction("Class name is not valid");

						if (allClasses.Contains(name))
							return new ValidatedAction("Class name already in use");

						if (toNewFile)
						{
							var relativePath = RelativeFilePath.TryParse(fileName);
							if (!relativePath.HasValue)
								return new ValidatedAction("Not a valid filename");

							if (!fileName.EndsWith(".ux"))
								return new ValidatedAction("Filename must end in .ux");

							if (fileSystem.Exists(projectPath.Combine(relativePath.Value)))
								return new ValidatedAction("The file already exists");

							if (fileName.IsNullOrEmpty())
								return new ValidatedAction("Filename can not be empty");
						}

						return new ValidatedAction(
							() =>
							{
								// A Task is returned by this method, however, we don't do anything with it
								// and just let it finish in the background
								classExtractor.ExtractClass(
									element: context.CurrentSelection,
									name: name,
									fileName: toNewFile ? Optional.Some(RelativeFilePath.Parse(fileName)) : Optional.None());
							});
					})
				.Replay(1).RefCount();

			_userInfo = action.Select(x => x.Message);

			_createCommand = Command.Create(action.Select(x => x.Action));
		}

		class ValidatedAction
		{
			public ValidatedAction(Action action)
			{
				Action = action;
				Message = "";
			}

			public ValidatedAction(string errorMessage)
			{
				Action = Optional.None();
				Message = errorMessage;
			}

			public override string ToString()
			{
				return string.Format("({0}, {1})", Action, Message);
			}

			public Optional<Action> Action { get; private set; }
			public string Message { get; private set; }
		}


		public Command CreateCommand
		{
			get { return _createCommand; }
		}

		public IProperty<string> ClassName
		{
			get { return _className; }
		}

		public IProperty<string> NewFileName
		{
			get { return _newFileName; }
		}

		public IProperty<bool> CreateInNewFile
		{
			get { return _createInNewFile; }
		}

		public IObservable<string> UserInfo
		{
			get { return _userInfo; }
		}
	}
}