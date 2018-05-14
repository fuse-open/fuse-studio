using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Outracks.Simulator.Protocol;
using Uno.UX.Markup;
using Uno.UX.Markup.Common;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.Parser
{
	public class UxParser
	{
		readonly string _project;
		readonly GhostCompilerFactory _ghostCompilerFactory;

		public UxParser(
			string project, 
			GhostCompilerFactory ghostCompilerFactory)
		{
			_project = project;
			_ghostCompilerFactory = ghostCompilerFactory;
		}

		/// <exception cref="UserCodeContainsErrors"></exception>
		public Project Parse(System.Collections.Immutable.ImmutableList<UxFileContents> documents, IMarkupErrorLog reporter)
		{
			const string restartSuggestion = "Restarting Fuse might be required for changes in Uno code to take effect";

			try
			{
				var ghostCompiler = _ghostCompilerFactory.CreateGhostCompiler(documents);
				var docs = Parse(documents, ghostCompiler, reporter);
				return docs;
			}
			catch (InvalidMarkup e)
			{
				reporter.ReportError(e.File, e.Position.HasValue ? e.Position.Value.Line : 1, e.Message);
				throw;
			}
			catch (CyclicClassHierarchy)
			{
				reporter.ReportError("UX class hierarchy contains cycles");
				throw;
			}
			catch (TypeNotFound e)
			{
				reporter.ReportError(e.File, 1, "Type " + e.TypeName + " not found (" + restartSuggestion + ")");
				throw;
			}
			catch (UnknownBaseClass e)
			{
				reporter.ReportError(e.DeclaringFile, 1, "Unknown base class " + e.BaseClassName + " for class " + e.DeclaredClassName + " (" + restartSuggestion + ")");
				throw;
			}
			catch (UnknownMemberType e)
			{
				reporter.ReportError(e.DeclaringFile, 1, "Unknown type `" + e.TypeName + "` for member `"+ e.MemberName+"` in " + e.DeclaringClassName + ".");
				throw;
			}
			catch(TypeNameCollision e)
			{
				reporter.ReportError("Multiple definitions of type `" + e.TypeName + "` in project");
				throw;
			}
			catch (UnknownError e)
			{
				// TODO: this reports an unavidable already reported NRE.. should get rid of that somehow
				reporter.ReportError(e.Message);
				throw;
			}	
		}

		/// <exception cref="UserCodeContainsErrors"></exception>
		Project Parse(IEnumerable<UxFileContents> documents, IDataTypeProvider ghostCompiler, IMarkupErrorLog log)
		{
			try
			{
				var markupErrorLog = new HasErrorsErrorLogWrapper(log);

				var result = Uno.UX.Markup.UXIL.Compiler.Compile(
					ghostCompiler,
					documents.Select(x => new Uno.UX.Markup.UXIL.Compiler.UXSource(x.Path, x.Contents)),
					Path.GetDirectoryName(_project),
					"Project",
					null,
					markupErrorLog);

				if (result == null)
					throw new UserCodeContainsErrors();

				if (markupErrorLog.HasErrors)
					throw new UserCodeContainsErrors();

				return result;

			}
			catch (UserCodeContainsErrors)
			{
				throw;
			}
			catch (XmlException e)
			{
				throw new InvalidMarkup(
					e.SourceUri,
					new TextPosition(new LineNumber(e.LineNumber + 1), new CharacterNumber(e.LinePosition + 1)),
					e.Message);
			}
			catch (MarkupException e)
			{
				throw new InvalidMarkup(
					e.Source,
					Optional.None(),
					e.Message);
			}
			catch (Exception e) // this dumb
			{
				throw new UnknownError(e.Message);
			}
		}


	}
}