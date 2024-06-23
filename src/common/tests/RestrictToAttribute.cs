using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Outracks.Diagnostics;

namespace Outracks.Tests
{
	public class RestrictToAttribute : Attribute, ITestAction
	{
		private readonly OS _os;

		public RestrictToAttribute(OS os)
		{
			_os = os;
		}

		public void BeforeTest(ITest test)
		{
			if (Platform.OperatingSystem != _os)
			{
				Assert.Ignore("Ignoring on " + Platform.OperatingSystem);
			}
		}

		public void AfterTest(ITest test) { }

		public ActionTargets Targets { get { return new ActionTargets(); } }
	}
}