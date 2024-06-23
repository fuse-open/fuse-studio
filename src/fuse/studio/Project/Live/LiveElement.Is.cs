using System;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Simulator;

namespace Outracks.Fuse.Live
{
	partial class LiveElement
	{
		public IObservable<bool> Is(string elementType)
		{
			return _metadata
				.Select(types =>
					Is(_elementId.Value, new ObjectIdentifier(elementType), types))
				.DistinctUntilChanged();
		}

		static bool Is(ObjectIdentifier type, ObjectIdentifier targetBaseType, ILookup<ObjectIdentifier, ObjectIdentifier> baseTypes)
		{
			if (type == targetBaseType)
				return true;

			foreach (var baseType in baseTypes[type])
				if (Is(baseType, targetBaseType, baseTypes))
					return true;

			return false;
		}

	}
}
