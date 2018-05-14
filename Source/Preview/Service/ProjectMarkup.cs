using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.Parser
{
	public sealed class ProjectMarkup
	{
		public readonly Project Project;
		public readonly ProjectBuild Build;

		public ProjectMarkup(Project project, ProjectBuild build)
		{
			Project = project;
			Build = build;
		}
	}
}