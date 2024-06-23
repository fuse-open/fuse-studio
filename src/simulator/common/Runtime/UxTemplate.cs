using Uno;
using Uno.UX;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	public class UxTemplate : Template
	{
		readonly Func<object> _create;

		public UxTemplate(
			Func<object> create,
			string matchCase,
			bool isDefault)
			: base (matchCase, isDefault)
		{
			_create = create;
		}

		public override object New()
		{
			return _create();
		}
	}
}
