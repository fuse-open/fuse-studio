using System.IO;

namespace Outracks.UnoHost
{
	public struct InputState
	{
		public readonly ModifierKeys Modifiers;
		public readonly MouseButtons PressedButons;

		public InputState(ModifierKeys modifiers, MouseButtons pressedButons)
		{
			Modifiers = modifiers;
			PressedButons = pressedButons;
		}

		public static void Write(BinaryWriter w, InputState s)
		{
			w.Write((int)s.Modifiers);
			w.Write((int)s.PressedButons);
		}

		public static InputState Read(BinaryReader r)
		{
			return new InputState(
				(ModifierKeys)r.ReadInt32(),
				(MouseButtons)r.ReadInt32());
		}
	}
}