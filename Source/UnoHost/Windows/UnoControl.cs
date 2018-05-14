using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Forms;
using OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using Uno;
using Uno.Diagnostics;
using Uno.Runtime.Implementation;
using Uno.Runtime.Implementation.Internal;
using ApplicationContext = Uno.ApplicationContext;

namespace Outracks.UnoHost.Windows
{
	class UnoControl : UserControl, IAppHost, IUnoCallbacks
	{
		readonly DpiAwareForm _form;
		readonly IReport _log;
		readonly PlatformWindowHandleImpl _unoWindow;
		readonly GraphicsContextHandleImpl _unoGraphics;		
		readonly GraphicsContext _glContext;			

		public PlatformWindowHandle GetPlatformWindow()
		{
			return _unoWindow;
		}

		public GraphicsContextHandle GetGraphicsContext()
		{
			return _unoGraphics;
		}

		public UnoControl(DpiAwareForm form, IReport log)
		{
			_form = form;
			_log = log;

			SetStyle(ControlStyles.Opaque, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			DoubleBuffered = false;
			Dock = DockStyle.Fill;

			// set up an OpenTK context
			Toolkit.Init(new ToolkitOptions { Backend = PlatformBackend.PreferNative });
			var windowInfo = Utilities.CreateWindowsWindowInfo(Handle);
			_glContext = ContextFactory.CreateContext(windowInfo);
			_glContext.SwapInterval = 0;

			_unoWindow = new PlatformWindowHandleImpl(this);
			_unoGraphics = new GraphicsContextHandleImpl(_unoWindow);


			_form.PreviewKeyDown += (sender, e) =>
			{
				// TODO: By doing this the tab key will not be sent to wpf at all, it should be treated as IsInputKey only when not handled by Uno. A better solution could be done by reading http://msdn.microsoft.com/en-us/library/ms742474%28v=vs.110%29.aspx and http://blogs.msdn.com/b/nickkramer/archive/2006/06/09/623203.aspx

				var codeWithoutModifiers = e.KeyCode & (~Keys.Control) & (~Keys.Shift);

				switch (codeWithoutModifiers)
				{
					case Keys.Left:
					case Keys.Right:
					case Keys.Up:
					case Keys.Down:
					case Keys.Tab:
						e.IsInputKey = true;
						break;
				}
			};

			Focus();
		}

		public void Initialize(UnoHostProject project, IObserver<OpenGlVersion> glObserver)
		{
			IGL gl = new OpenTKGL();
#if DEBUG
			GL.Initialize(gl, true);
#else
			GL.Initialize(gl, false);
#endif
			ApplicationContext.Initialize(this);
			glObserver.OnNext(
				new OpenGlVersion(
					gl.GetString(GLStringName.Version),
					gl.GetString(GLStringName.Vendor),
					gl.GetString(GLStringName.Renderer)));


			// Hook up size / dpi changed

			_form.DpiChanged += (s, a) =>
			{
				_unoWindow.ChangeSize(Size.ToPixelSize(), _form.Density);
				PerformRepaint();
			};

			Resize += (s, a) =>
			{
				_unoWindow.ChangeSize(Size.ToPixelSize(), _form.Density);
				PerformRepaint();
			};

			_form.Closed += (s, a) => Bootstrapper.OnAppTerminating(_unoWindow);


			// Hook up mouse events

			MouseDown += (s, a) =>
				_log.TrySomethingBlocking(
					() =>
					{
						if (a.Button == System.Windows.Forms.MouseButtons.Right)
							return;

						Bootstrapper.OnMouseDown(_unoWindow, a.X, a.Y, a.Button.ToUno());
					});

			MouseUp += (s, a) =>
				_log.TrySomethingBlocking(() => Bootstrapper.OnMouseUp(_unoWindow, a.X, a.Y, a.Button.ToUno()));

			MouseMove += (s, a) =>
				_log.TrySomethingBlocking(() => Bootstrapper.OnMouseMove(_unoWindow, a.X, a.Y));

			//control.MouseEnter += (s, a) =>
			//	log.TrySomethingBlocking(() => toApp.OnPointerEvent(new PointerEnterEventArgs(WinFormsInputState.Query())));

			MouseLeave += (s, a) =>
				_log.TrySomethingBlocking(() => Bootstrapper.OnMouseOut(_unoWindow));

			MouseWheel += (s, a) =>
			{
				var numLinesPerScroll = SystemInformation.MouseWheelScrollLines;
				var deltaMode = numLinesPerScroll > 0
					? Uno.Platform.WheelDeltaMode.DeltaLine
					: Uno.Platform.WheelDeltaMode.DeltaPage;

				var delta = deltaMode == Uno.Platform.WheelDeltaMode.DeltaLine
					? (a.Delta / 120.0f) * (float)numLinesPerScroll
					: a.Delta / 120.0f;

				_log.TrySomethingBlocking(() => Bootstrapper.OnMouseWheel(_unoWindow, 0, delta, (int)deltaMode));
			};


			// Hook up keyboard events

			KeyDown += (s, a) =>
				_log.TrySomethingBlocking(() =>
					a.Handled = a.KeyCode.ToKey().MatchWith(
						key => Bootstrapper.OnKeyDown(_unoWindow, key),
						() => false));

			KeyUp += (s, a) =>
				_log.TrySomethingBlocking(() =>
					a.Handled = a.KeyCode.ToKey().MatchWith(
						key => Bootstrapper.OnKeyUp(_unoWindow, key),
						() => false));

			KeyPress += (s, a) =>
				_log.TrySomethingBlocking(() =>
				{
					if (!char.IsControl(a.KeyChar))
						a.Handled = Bootstrapper.OnTextInput(_unoWindow, a.KeyChar.ToString());
				});

			_unoWindow.ChangeSize(_form.Size.ToPixelSize(), _form.Density);
		}
	

		protected override CreateParams CreateParams
		{
			get
			{
				const int CS_VREDRAW = 0x1;
				const int CS_HREDRAW = 0x2;
				const int CS_OWNDC = 0x20;

				var cp = base.CreateParams;

				// Setup necessary class style for OpenGL on windows
				cp.ClassStyle |= CS_VREDRAW | CS_HREDRAW | CS_OWNDC;

				return cp;
			}
		}

		readonly Stopwatch _timer = Stopwatch.StartNew();
		readonly ISubject<double> _perFrame = new Subject<double>();
		public IObservable<double> PerFrame { get { return _perFrame; } }

		/*
		 * STAThread will run the message pumping system in WinForms when the main thread is waiting on a wait handle
		 * So it basically 'yields', which can make 'PerformRepaint' to run again during 'Bootstrapper.OnUpdate' or 'Bootstrapper.OnDraw'
		 * which fuselibs doesn't expect of course. So yeah, Windows COM pattern sucks at times. It could be solved by listening for Paint on the control
		 * however the performance isn't as good as current approach. 
		 */
		bool _isInFuselibs = false; 
		public void PerformRepaint()
		{
			if (_isInFuselibs)
				return;

			try
			{			
				if (_unoWindow.Size.HasZeroArea())
					return;

				if (Uno.Application.Current == null)
					return;

				_isInFuselibs = true;
				_perFrame.OnNext(_timer.Elapsed.TotalSeconds);
				_log.TrySomethingBlocking(Bootstrapper.OnUpdate);

				GL.ClearDepth(1);
				GL.Clear(GLClearBufferMask.ColorBufferBit | GLClearBufferMask.DepthBufferBit);

				_log.TrySomethingBlocking(Bootstrapper.OnDraw);

				_glContext.SwapBuffers();
			}
			finally
			{
				_isInFuselibs = false;
			}
		}

		public void SetCursor(Cursor toCursor) { }

		public void Run()
		{
			_form.MainLoop(PerformRepaint);
		}
	}
}
