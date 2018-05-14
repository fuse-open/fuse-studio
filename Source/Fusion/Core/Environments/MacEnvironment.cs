using System;
using System.Collections.Generic;
using Outracks.IO;

namespace Outracks.Fusion
{
	public static class MacEnvironment
	{
		static IMacEnvironmentImpl _implementation;

		static IMacEnvironmentImpl Implementation
		{
			get
			{
				if (_implementation == null)
				{
					throw new InvalidOperationException("Mac specific bindings not initialized or available.");
				}

				return _implementation;
			}
		}

		public static IEnumerable<AbsoluteDirectoryPath> GetPathToApplicationsThatContains(string name)
		{
			return Implementation.GetPathToApplicationsThatContains(name);
		}

		public static IEnumerable<AbsoluteDirectoryPath> GetApplications(string identifier)
		{
			return Implementation.GetApplications(identifier);
		}

		public static void Initialize(IMacEnvironmentImpl impl)
		{
			_implementation = impl;
		}
	}

	public interface IMacEnvironmentImpl
	{
		IEnumerable<AbsoluteDirectoryPath> GetPathToApplicationsThatContains(string name);
		IEnumerable<AbsoluteDirectoryPath> GetApplications(string identifier);
	}
}
