using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;

namespace Outracks.UnoDevelop.UXNinja
{
	static class MemberHelper
	{
		public static IEnumerable<Member> GetAllPublicEvents(DataType dataType)
		{
			if (dataType.Base != null)
				foreach(var evt in GetAllPublicEvents(dataType.Base))
					yield return evt;

			foreach (var evt in dataType.Events.Where(e => e.IsPublic))
				yield return evt;
		}
	}
}