using System;
using System.Linq;
using System.Reflection;
//using Foundation;
//using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	// FIXME: Move to Mac-specific test project.

	/*[TestFixture]
	public class NSObjectTest
	{
		[Test]
		public void TestIfAllNSObjectsGotAnIntPtrConstructor()
		{
			var fusionAsm = Assembly.Load("Outracks.Fusion.Mac");
			bool failedOnce = false;
			foreach (var type in fusionAsm.GetTypes())
			{
				// If the type derives from a NSObject, it should have a IntPtr constructor.
				if (type.IsSubclassOf(typeof(NSObject)) && type.GotAnIntPtrConstructor() == false)
				{
					Console.WriteLine("Type: " + type + " derives a NSObject however doesn't have an IntPtr constructor.");
					failedOnce = true;
				}
			}

			Assert.False(failedOnce);
		}
	}*/

	public static class ReflectionExtensions
	{
		public static bool GotAnIntPtrConstructor(this Type type)
		{
			var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return constructors.Any(
				constructor =>
				{
					var parameters = constructor.GetParameters();
					if (parameters.Length != 1)
						return false;

					var firstParam = parameters[0];
					return firstParam.ParameterType == typeof(IntPtr);
				});
		}
	}
}
