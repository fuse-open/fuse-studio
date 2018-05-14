using System.Collections.Generic;
using Uno.Build;

namespace Fuse.Preview
{
	public interface IErrorHelper
	{
		void OnBuildFailed(BuildResult build);
	}

	public class CombinedErrorHelper : IErrorHelper
	{
		public IEnumerable<IErrorHelper> Helpers { get; set; }

		public void OnBuildFailed(BuildResult build)
		{
			foreach (var helper in Helpers)
				helper.OnBuildFailed(build);
		}
	}
}