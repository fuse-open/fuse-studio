using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody2 : Test
	{
		[Test]
		public void MethodBody200()
		{
			AssertCode(
@"using Uno;

class Stack<T>
{
	T[] data;
	int index;

	public Stack(int size)
	{
		if (size <= 0) throw new Exception(""Stack size must be positive and greater than zero"");
		data = new T[size];
		index = 0;
	}

	public void Push(T item)
	{
		if (index >= data.$(Length)
	}

	public T Pop()
	{
		if (index <= 0) throw new Exception(""Stack underflow occurred"");
		return data[--index];
	}
}

"
			);
		}

		[Test]
		public void MethodBody201()
		{
			AssertCode(
@"

using Uno;

class Interpreter
{
	public void Run(string[] input)
	{
		if (input.Length > 25) throw new Exception(""Input must be 25 or less characters tall"");
		foreach (var s in input)
		{
			if (s.Length > 80) throw new Exception(""Input must be 80 or less characters wide"");
		}

		string[] grid = new String[25];
		for (int i = 0; i < 25; i++)
		{
			grid[i] = """";
			for (int j = 0; j < 80; j++)
			{
				if (i < input.Length && j < input[i].Length)
				{
					grid[i] += input[i][j];
				}
				else
				{
					grid[i] += ' ';
				}
			}
		}

		var ip = int2(0);
		var ipDir = int2(1, 0);
		var stack = new Stack<$(int)
		while (true)
		{
			switch (grid[ip.Y][ip.X])
			{

			}
			ip += ipDir;

			break;
		}
	}
}

"
			);
		}

		/*[Test]
		public void MethodBody202()
		{
			AssertCode(
@"

using Uno;
using Uno.UI;

namespace StandardLib.UI
{
	class PointerEventTest : Test
	{
		class SomeClass
		{
			public event PointerEventHandler SomePointerEvent;

			public void OnSomePointerEvent(float2 pos, $(PointerEventType))
			{
				if (SomePointerEvent != null) SomePointerEvent(this, new PointerEventArgs(
			}
		}

		public PointerEventTest()
			: base(""PointerEventTest"")
		{
		}

		public override void Run()
		{
			var a = new PointerEventArgs(float2(1.0f, 2.0f), PointerEventType.Click);
			assert(a is PointerEventArgs);
			assert(a is EventArgs);
			assert(compare(a.Position, float2(1.0f, 2.0f)));
			assert(a.Type == PointerEventType.Click);
			assert(compare(a.WorldPosition, float3(0.0f)));
			assert(!a.Handled);
		}
	}
}

"
			);
		}*/

		[Test]
		public void MethodBody203()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.IO;

namespace StandardLib.IO
{
	class WriterTest : Test
	{
		class SomeWriter : Writer
		{
			byte[] internalData;
			bool isOpen = true;
			int position = 0;

			public override int Length { get { return internalData.Length; } }
			public override int Position { get { return position; } }

			public SomeWriter(int size)
			{
				internalData = new byte[size];
			}

			void addByte(byte b)
			{
				if (!isOpen) throw new Exception(""Stream is closed"");
				if (Position >= Length) throw new Exception(""Buffer overflow has occurred"");
			}

			public override void Close()
			{
				isOpen = false;
			}

			public override void Write(byte[] data)
			{
				foreach (var b in data) addByte(b);
			}

			public override void Write(bool data)
			{
				Write(data ? 1 : 0);
			}

			public override void Write(byte data)
			{
				addByte(data);
			}

			public override void Write(double data)
			{
				for (int i = 0; i < 8; i++) addByte(0);
			}

			public override void Write(float data)
			{
				for (int i = 0; i < 4; i++) addByte(0);
			}

			public override void Write(int data)
			{
				for (int i = 0; i < 4; i++) addByte(0);
			}

			public override void Write(long data)
			{
				for (int i = 0; i < 8; i++) addByte(0);
			}

			public override void Write(sbyte data)
			{
				for (int i = 0; i < 8; i++) addByte(0);
			}

			public override void Write(short data)
			{
				for (int i = 0; i < 2; i++) addByte(0);
			}

			public override void Write(uint data)
			{
				for (int i = 0; i < 4; i++) addByte(0);
			}

			public override void Write(ulong data)
			{
				for (int i = 0; i < 8; i++) addByte(0);
			}

			public override void Write(ushort data)
			{
				for (int i = 0; i < 2; i++) addByte(0);
			}
		}

		public WriterTest()
			: base(""WriterTest"")
		{
		}

		public void TestWriter(Writer w)
		{
			$(w)

			var originalLength = w.Length;
			assert(w.Length == originalLength);
			assert(w.Position == 0);

			w.Write(new byte[] { 1, 2, 3, 4, 5, 6 });
			assert(w.Length == originalLength);
			assert(w.Position == 6);

			w.Write(false);
			assert(w.Length == originalLength);
			assert(w.Position == 10);

			w.Write((byte)12);
			assert(w.Length == originalLength);
			assert(w.Position == 11);

			w.Write(4.0);
			assert(w.Length == originalLength);
			assert(w.Position == 19);

			w.Write(2.4f);
			assert(w.Length == originalLength);
			assert(w.Position == 23);

			w.Write(5);
			assert(w.Length == originalLength);
			assert(w.Position == 27);

			w.Write((long)12343344);
			assert(w.Length == originalLength);
			assert(w.Position == 35);

			w.Write((sbyte)-12);
			assert(w.Length == originalLength);
			assert(w.Position == 36);

			w.Write((short)4);
			assert(w.Length == originalLength);
			assert(w.Position == 38);

			w.Write((uint)3244444);
			assert(w.Length == originalLength);
			assert(w.Position == 42);

			w.Write((ulong)23494949999);
			assert(w.Length == originalLength);
			assert(w.Position == 50);

			w.Write((ushort)6);
			assert(w.Length == originalLength);
			assert(w.Position == 52);

			w.Write(byte2(23, 255));
			assert(w.Length == originalLength);
			assert(w.Position == 54);

			w.Write(byte4(1, 2, 3, 4));
			assert(

			w.Close();
		}

		public override void Run()
		{
			TestWriter(new SomeWriter(200));
		}
	}
}

"
			);
		}

		/*[Test]
		public void MethodBody204()
		{
			AssertCode(
@"

using Uno;
using Uno.UI;

namespace StandardLib.UI
{
	class WindowTest : Test
	{
		class SomeClass
		{
			public event ResizeEventHandler Resize;
			public event ClosingEventHandler Closing;

			public void OnResize()
			{
				if (Resize != null) Resize(this, EventArgs.Empty);
			}

			public void OnClosing(ClosingEventArgs e)
			{
				if (Closing != null) Closing(this, e);
			}
		}

		class CrapWindow : Window
		{
			public CrapWindow()
			{
				IsSoftKeyboardShowing = false;
			}

			public override void Close()
			{
			}

			public override string Title { get; set; }
			public override MouseCursor Cursor { get; set; }
			public override int2 Size { get; set; }
			public override bool Fullscreen { get; set; }

			public override bool HasSoftKeyboard { get { return true; } }

			public bool IsSoftKeyboardShowing { get; private set; }
			public override void ShowSoftKeyboard(KeyboardType type)
			{
				IsSoftKeyboardShowing = true;
			}

			public override void HideSoftKeyboard()
			{
				IsSoftKeyboardShowing = false;
			}
		}

		bool resizeHandled = false;
		void resizeHandler(object sender, EventArgs e)
		{
			resizeHandled = true;
		}

		bool closingHandled = false;
		void closingHandler(object sender, ClosingEventArgs e)
		{
			closingHandled = e.Cancel;
		}

		public WindowTest()
			: base(""WindowTest"")
		{
		}

		bool mouseDownHandled = false;
		void mouseDownHandler(object sender, MouseEventArgs e)
		{
			mouseDownHandled = true;
		}

		bool mouseUpHandled = false;
		void mouseUpHandler(object sender, MouseEventArgs e)
		{
			mouseUpHandled = true;
		}

		bool mouseMoveHandled = false;
		void mouseMoveHandler(object sender, MouseEventArgs e)
		{
			mouseMoveHandled = true;
		}

		bool mouseWheelHandled = false;
		void mouseWheelHandler(object sender, MouseEventArgs e)
		{
			mouseWheelHandled = true;
		}

		bool touchDownHandled = false;
		void touchDownHandler(object sender, TouchEventArgs e)
		{
			touchDownHandled = true;
		}

		bool touchUpHandled = false;
		void touchUpHandler(object sender, TouchEventArgs e)
		{
			touchUpHandled = true;
		}

		bool touchMoveHandled = false;
		void touchMoveHandler(object sender, TouchEventArgs e)
		{
			touchMoveHandled = true;
		}

		bool keyDownHandled = false;
		void keyDownHandler(object sender, KeyEventArgs e)
		{
			keyDownHandled = true;
		}

		bool keyUpHandled = false;
		void keyUpHandler(object sender, KeyEventArgs e)
		{
			keyUpHandled = true;
		}

		public override void Run()
		{
			var a = new ClosingEventArgs();
			assert(a is ClosingEventArgs);
			assert(a is EventArgs);
			assert(!a.Cancel);

			var b = new SomeClass();
			assert(!resizeHandled);
			b.OnResize();
			assert(!resizeHandled);
			b.Resize += resizeHandler;
			assert(!resizeHandled);
			b.OnResize();
			assert(resizeHandled);

			assert(!closingHandled);
			b.OnClosing(new ClosingEventArgs());
			assert(!closingHandled);
			b.Closing += closingHandler;
			assert(!closingHandled);
			b.OnClosing(new ClosingEventArgs());
			assert(!closingHandled);
			var c = new ClosingEventArgs();
			c.Cancel = true;
			b.OnClosing(c);
			assert(closingHandled);

			Window d = new CrapWindow();
			assert(d is Window);
			d.Close();
			d.Title = ""hi mom"";
			assert(d.Title == ""hi mom"");
			d.Cursor = MouseCursor.Crosshair;
			assert(d.Cursor == MouseCursor.Crosshair);
			d.Cursor = MouseCursor.ResizeNorth;
			assert(d.Cursor == MouseCursor.ResizeNorth);
			d.Cursor = MouseCursor.Progress;
			assert(d.Cursor == MouseCursor.Progress);
			d.Size = int2(500, 400);
			assert(d.Size == int2(500, 400));
			d.Fullscreen = true;
			assert(d.Fullscreen);
			d.Fullscreen = false;
			assert(!d.Fullscreen);
			assert(d.HasSoftKeyboard);
			assert(!((CrapWindow)d).IsSoftKeyboardShowing);
			d.ShowSoftKeyboard(KeyboardType.Phone);
			assert(((CrapWindow)d).IsSoftKeyboardShowing);
			d.HideSoftKeyboard();
			assert(!((CrapWindow)d).IsSoftKeyboardShowing);
			resizeHandled = false;
			assert(!mouseDownHandled);
			assert(!mouseUpHandled);
			assert(!mouseMoveHandled);
			assert(!mouseWheelHandled);
			assert(!touchDownHandled);
			assert(!touchUpHandled);
			assert(!touchMoveHandled);
			assert(!keyDownHandled);
			assert(!keyUpHandled);
			assert(!resizeHandled);
			d.OnMouseDown(new MouseEventArgs(2, 4));
			d.OnMouseUp(new MouseEventArgs(-12, 8444));
			d.OnMouseMove(new MouseEventArgs(566, 34));
			d.OnMouseWheel(new MouseEventArgs(234, 234));
			d.OnTouchDown(new TouchEventArgs(0, float2(.4f, .5f)));
			d.OnTouchUp(new TouchEventArgs(667, float2(.8f, .2f)));
			d.OnTouchMove(new TouchEventArgs(-7, float2(.04f, .244f)));
			d.OnKeyDown(new KeyEventArgs(Keys.B));
			d.OnKeyUp(new KeyEventArgs(Keys.Space));
			d.OnResize(EventArgs.Empty);
			assert(!mouseDownHandled);
			assert(!mouseUpHandled);
			assert(!mouseMoveHandled);
			assert(!mouseWheelHandled);
			assert(!touchDownHandled);
			assert(!touchUpHandled);
			assert(!touchMoveHandled);
			assert(!keyDownHandled);
			assert(!keyUpHandled);
			assert(!resizeHandled);
			d.MouseDown += mouseDownHandler;
			d.MouseUp += mouseUpHandler;
			d.MouseMove += mouseMoveHandler;
			d.MouseWheel += mouseWheelHandler;
			d.TouchDown += touchDownHandler;
			d.TouchUp += touchUpHandler;
			d.TouchMove += touchMoveHandler;
			d.KeyDown += keyDownHandler;
			d.KeyUp += keyUpHandler;
			d.Resize += resizeHandler;
			d.OnMouseDown(new MouseEventArgs(2, 4));
			d.OnMouseUp(new MouseEventArgs(-12, 8444));
			d.OnMouseMove(new MouseEventArgs(566, 34));
			d.OnMouseWheel(new MouseEventArgs(234, 234));
			d.OnTouchDown(new TouchEventArgs(0, float2(.4f, .5f)));
			d.OnTouchUp(new TouchEventArgs(667, float2(.8f, .2f)));
			d.OnTouchMove(new TouchEventArgs(-7, float2(.04f, .244f)));
			d.OnKeyDown(new KeyEventArgs(Keys.B));
			d.OnKeyUp(new KeyEventArgs(Keys.Space));
			d.OnResize(EventArgs.Empty);
			assert(mouseDownHandled);
			assert(mouseUpHandled);
			assert(mouseMoveHandled);
			assert(mouseWheelHandled);
			assert(touchDownHandled);
			assert(touchUpHandled);
			assert(touchMoveHandled);
			assert(keyDownHandled);
			assert(keyUpHandled);
			assert(resizeHandled);
			mouseDownHandled = mouseUpHandled = mouseMoveHandled = mouseWheelHandled = false;
			touchDownHandled = touchUpHandled = touchMoveHandled = false;
			keyDownHandled = keyUpHandled = false;
			resizeHandled = false;
			assert(!mouseDownHandled);
			assert(!mouseUpHandled);
			assert(!mouseMoveHandled);
			assert(!mouseWheelHandled);
			assert(!touchDownHandled);
			assert(!touchUpHandled);
			assert(!touchMoveHandled);
			assert(!keyDownHandled);
			assert(!keyUpHandled);
			assert(!resizeHandled);
			WindowHelpers.EmulateMouseAsTouchEvents(d);
			d.OnMouseDown(new MouseEventArgs(3, 4));
			d.OnMouseMove(new MouseEventArgs(6, 8));
			d.OnMouseUp(new MouseEventArgs(1, 1));
			assert(mouseDownHandled);
			assert(mouseUpHandled);
			assert(mouseMoveHandled);
			assert(touchDownHandled);
			assert(touchUpHandled);
			assert(touchMoveHandled);
			mouseDownHandled = mouseUpHandled = mouseMoveHandled = false;
			touchDownHandled = touchUpHandled = touchMoveHandled = false;
			var e = new CrapWindow();
			e.OnTouchDown(new TouchEventArgs(4, float2(4.0f, 5.0f)));
			e.OnTouchMove(new TouchEventArgs(4, float2(8.0f, 2.0f)));
			e.OnTouchUp(new TouchEventArgs(4, float2(6.2f, 87.0f)));
			assert(!mouseDownHandled);
			assert(!mouseUpHandled);
			assert(!mouseMoveHandled);
			assert(!touchDownHandled);
			assert(!touchUpHandled);
			assert(!touchMoveHandled);
			e.($MouseDown, !Handled)
			WindowHelpers.EmulateTouchAsMouseEvents(e);
			e.OnTouchDown(new TouchEventArgs(4, float2(4.0f, 5.0f)));
			e.OnTouchMove(new TouchEventArgs(4, float2(8.0f, 2.0f)));
			e.OnTouchUp(new TouchEventArgs(4, float2(6.2f, 87.0f)));
			assert(mouseDownHandled);
			assert(mouseUpHandled);
			assert(mouseMoveHandled);
			assert(touchDownHandled);
			assert(touchUpHandled);
			assert(touchMoveHandled);
		}
	}
}

"
			);
		}*/

		[Test]
		public void MethodBody205()
		{
			AssertCode(
@"

namespace StandardLib.Scenes
{
	class DistanceSorterTest : Test
	{
		public DistanceSorterTest()
			: base(""DistanceSorterTest"")
		{
		}

		public override void Run()
		{
			var a = new DistanceSorter<$(int)
		}
	}
}

"
            );
		}

		[Test]
		public void MethodBody206()
		{
			AssertCode(
@"

class FOo
{
	void Bar()
	{
		null.$(!Uno, !if, !null)
	}
}

"
			);
		}

		[Test]
		public void MethodBody207()
		{
			AssertCode(
@"

namespace StandardLib.Scenes
{
	class AnimationTest : Test
	{
		class BasicAnimation : Animation
		{
			double[] times = new [] { 0.0, .1, 2, .3 };
			double[] values = new [] { 1.0, 2.0, 3.0, 4.0 };
			TransitionType[] transitionTypes = new [] { TransitionType.Linear, TransitionType.Linear, TransitionType.Linear, TransitionType.Linear };
			TransitionModifier[] transitionModifiers = new [] { TransitionModifier.None, TransitionModifier.None, TransitionModifier.None, TransitionModifier.None };

			public override double GetLength()
			{
				return .4;
			}

			public float CurrentValue = default(float);

			protected override void update(double time)
			{
				CurrentValue = (float)getValue(time, values, values, times, transitionTypes, transitionModifiers, values.Length);
			}
		}

		public AnimationTest()
			: base(""AnimationTest"")
		{
		}

		public override void Run()
		{
			var a = new BasicAnimation();
			$(assert)
		}
	}
}"
            );
		}

	}
}