using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Studio
{
	internal static class RespondToFocusRequests
	{
		private static IObservable<IEnumerable<KeyValuePair<AbsoluteFilePath, AbsoluteFilePath>>> ToProjectDocMap(
			ProjectHost projectHost)
		{
			var project = projectHost.Project;
			var docs = project.Documents;
			var docPaths = docs.Select(x => x.Select(y => y.FilePath).ToObservableEnumerable()).Switch();
			var projectPath = project.FilePath;
			return projectPath.CombineLatest(
				docPaths,
				(pp, dp) => dp.Select(x => new KeyValuePair<AbsoluteFilePath, AbsoluteFilePath>(x, pp)));
		}

		public static IDisposable Start(
			IMessagingService daemon,
			IObservable<ImmutableList<ProjectHost>> projectHostListStream)
		{
			return projectHostListStream
				.Select(
					projectHosts =>
						projectHosts
							.Select(ToProjectDocMap)
							.CombineLatest(projectDocMap => projectDocMap.SelectMany(mapEntry => mapEntry)))
				.Switch()
				.SubscribeUsing(
					docToProjectMap =>
					{
						return daemon.ProvideOptionally<FocusDesignerRequest, FocusDesignerResponse>(
							x =>
							{
								var projectPath =
									docToProjectMap.Where(
											y => string.Equals(y.Key.NativePath, x.File, StringComparison.OrdinalIgnoreCase))
										.Select(y => y.Value)
										.FirstOrDefault();

								if (projectPath != null)
								{
									Application.OpenDocument(projectPath, true);
									return new FocusDesignerResponse();
								}
								return new Optional<FocusDesignerResponse>();
							});
					});
		}
	}
}