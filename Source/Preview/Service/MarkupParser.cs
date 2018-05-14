using System;
using System.Collections.Immutable;
using Fuse.Preview;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.Parser
{
	using Protocol;

	public class MarkupParser 
	{
		readonly UxParser _parser;
		readonly ProjectBuild _build;
		
		public MarkupParser(ProjectBuild build, UxParser parser)
		{
			_parser = parser;
			_build = build;
		}

		public Optional<ProjectMarkup> TryParseDocuments(GenerateBytecode a, IObserver<IBinaryMessage> buildEvents, Guid buildId)
		{
			try
			{
				return new ProjectMarkup(ParseDocument(ImmutableList.ToImmutableList(a.UxFiles), buildEvents, buildId), _build);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="InvalidMarkup"></exception>
		/// <exception cref="CyclicClassHierarchy"></exception>
		/// <exception cref="TypeNotFound"></exception>
		/// <exception cref="UnknownBaseClass"></exception>
		/// <exception cref="UnknownMemberType"></exception>
		/// <exception cref="UnknownError"></exception>
		/// <exception cref="UserCodeContainsErrors">(Base exception type for all exceptions above)</exception>
		/// <exception cref="TypeNameCollision"></exception>
		public Project ParseDocument(System.Collections.Immutable.ImmutableList<UxFileContents> documents, IObserver<IBinaryMessage> buildEvents, Guid buildId)		
		{
			return _parser.Parse(
				documents: documents,
				reporter: new MarkupErrorLog(buildEvents, buildId));
		}

	}
}