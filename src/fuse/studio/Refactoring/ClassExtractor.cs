using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Refactoring
{
	public class ClassExtractor : IClassExtractor
	{
		readonly IProject _project;
		readonly ISubject<string> _logMessages = new Subject<string>();

		public ClassExtractor(IProject project)
		{
			_project = project;
		}

		public IObservable<string> LogMessages
		{
			get { return _logMessages; }
		}

		public async Task ExtractClass(IElement element, string name, Optional<RelativeFilePath> fileName)
		{
			try
			{
				if (fileName.HasValue)
				{
					await ExtractClassToFile(element, name, fileName.Value);
					return;
				}

				element.UxClass().Write(name);
				element.PasteAfter(SourceFragment.FromString(string.Format("<{0} />", name)));
			}
			catch (Exception ex)
			{
				_logMessages.OnNext(string.Format("Error: Unable to create class. {0}\r\n", ex.Message));
			}
		}

		async Task ExtractClassToFile(IElement element, string name, RelativeFilePath fileName)
		{
			var xml = (await element.Copy()).ToXml();
			xml.SetAttributeValue(KeyToName("ux:Class"), name);
			await _project.CreateDocument(fileName, SourceFragment.FromXml(xml));
			// Don't replace original element before new class is loaded
			await _project.Classes
				.WherePerElement(x => x.UxClass().Is(name))
				.Where(x => x.Any())
				.FirstAsync()
				.Timeout(TimeSpan.FromSeconds(2));
			await element.Replace(_ => Task.FromResult(SourceFragment.FromString(string.Format("<{0} />", name))));
		}

		static XName KeyToName(string name)
		{
			return name.StartsWith("ux:")
				? XName.Get(name.StripPrefix("ux:"), "http://schemas.fusetools.com/ux")
				: XName.Get(name);
		}
	}
}