using Uno;
using Uno.IO;
using Uno.Collections;
using Fuse;
using Uno.Diagnostics;
using Uno.UX;
using Fuse.Controls;
using Fuse.Elements;

namespace Outracks.Simulator.Client
{
	using Bytecode;
	using Protocol;
	using Runtime;

	sealed class UserAppState
	{
		// This list should be up-to-date with https://www.fusetools.com/docs/fuse/app

		public readonly float4 Background;
		public readonly Node[] Children;
		// (Current)
		// (PreviousUpdateDuration)
		public readonly Resource[] Resources;
		public readonly float4 ClearColor;
		// FrameInterval?
		// FrameTime?

		public UserAppState(
			float4 background,
			Node[] children,
			Resource[] resources,
			float4 clearColor)
		{

			Background = background;
			Children = children;
			Resources = resources;
			ClearColor = clearColor;
		}

		public static UserAppState Default { get; set; }

		public static UserAppState Save(FakeApp app)
		{
			return new UserAppState(
				app.Background,
				app.Children.ToArray(),
				app.Resources.ToArray(),
				app.ClearColor);
		}

		public void ApplyTo(FakeApp app)
		{
			if (app.Background != Background) app.Background = Background;
			SetIfNotEqual(app.Children, Children);
			SetIfNotEqual(app.Resources, Resources);
			if (app.ClearColor != ClearColor) app.ClearColor = ClearColor;
		}

		static void SetIfNotEqual<T>(IList<T> list, T[] elements)
		{
			if (!SequenceEquals(list, elements))
				SetSequence(list, elements);
		}

		static void SetSequence<T>(IList<T> list, T[] elements)
		{
			list.Clear();
			list.AddRange(elements);
		}

		static bool SequenceEquals<T>(IList<T> left, T[] right)
		{
			if (left.Count != right.Length) 
				return false;

			for (int i = 0; i < right.Length; i++)
			{
				if ((object)left[i] != (object)right[i]) 
					return false;
			}
			return true;
		}
	}
}
