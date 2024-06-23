using System;
using Uno;
using Uno.IO;
using Uno.UX;
using Uno.Net;
using Uno.Collections;
using Fuse;
using Uno.Diagnostics;
using Fuse.Controls;
using Fuse.Elements;
using Fuse.Input;

namespace Outracks.UnoHost
{
	public class FusionInterop
	{
		public static void OnPointerPressed(object obj, Action<float2> handler)
		{
			var visual = obj as Element;
			if (visual == null)
				return;

			visual.HitTestMode = HitTestMode.LocalBounds;
			Pointer.Pressed.AddHandler(visual, new FusionInterop { _handler = handler }.Handler);
		}

		public static void OnPointerMoved(object obj, Action<float2> handler)
		{
			var visual = obj as Element;
			if (visual == null)
				return;

			visual.HitTestMode = HitTestMode.LocalBounds;
			Pointer.Moved.AddHandler(visual, new FusionInterop { _handler = handler }.Handler);
		}

		Action<float2> _handler;

		void Handler(object sender, PointerPressedArgs args)
		{
			_handler(args.WindowPoint);
		}
		
		void Handler(object sender, PointerMovedArgs args)
		{
			_handler(args.WindowPoint);
		}

		public static IList<object> HitTest(float2 point)
		{
			var visual = Outracks.Simulator.Client.Context.App;
			var query = new HitTestQuery();
			var args = new HitTestContext(point, query.Select);
			visual.HitTest(args);
			args.Dispose();
			return query.HitObjects;
		}
	}

	class HitTestQuery
	{
		public readonly List<object> HitObjects = new List<object>(); 
		public void Select(HitTestResult result)
		{
			HitObjects.Add(result.HitObject);
		}
	}
}

namespace Outracks.Simulator.Client
{
	using Runtime;
	using Bytecode;
	using Protocol;
	using Runtime;
	using Fuse;
	using Fuse.Input;
	
	
	public class Application : Fuse.App, IPointerEventResponder
	{
		State _state = new Uninitialized();
		IPointerEventResponder _defaultPointerEventResponder;
		readonly ConcurrentQueue<Action> _dispatcher = new ConcurrentQueue<Action>();

		public IReflection Reflection { get; set; }

		public Application(IPEndPoint[] proxyEndpoints, string project, string[] defines)
		{
			Context.SetGlobals(proxyEndpoints, project, defines);

			_defaultPointerEventResponder = Pointer.EventResponder;
			Pointer.EventResponder = this;
			UnhandledException += OnUnhandledException;

			var fakeApp = new FakeApp(this);
			UserAppState.Default = UserAppState.Save(fakeApp);

			if defined(CPLUSPLUS)
				ResetEverything(true, new Panel());

			_developerMenu = new Outracks.Simulator.DeveloperMenu();
		}

		public void ResetEverything(bool initial, object overlay = null)
		{
			var fakeApp = new FakeApp(this);

			Context.SetApp(fakeApp);

			try
			{
				foreach (var panel in Children)
					((Panel)panel).Children.Clear();

				Children.Clear();
			} 
			catch( Exception e ) 
			{
				Fuse.Diagnostics.UnknownException("Failed to properly reset. Try exiting Fuse and restarting.",
					e, this );
			}
			
			var p = new Panel();

			if (overlay != null)
				p.Children.Add((Node)overlay);
			else if (Children.Count > 0 && ((Panel)Children[0]).Children.Count > 0)
				p.Children.Add((Node)((Panel)Children[0]).Children[0]);

			p.Children.Add(fakeApp);
			Children.Add(p);
		}


		// Detect three-finger left swipe for developer menu
		List<PointerPressedArgs> pointers = new List<PointerPressedArgs>();
		
		public void OnPointerPressed(PointerPressedArgs args)
		{
			if defined(!DOTNET)
			{
				pointers.Add(args);
				if (pointers.Count == 3)
					Fuse.Timer.Wait(1, ShowDeveloperMenu);
			}
			
			//SetState(_state.OnPointerPressed(args));
			//if (!args.IsHandled)
				_defaultPointerEventResponder.OnPointerPressed(args);
		}

		Outracks.Simulator.DeveloperMenu _developerMenu;

		void ShowDeveloperMenu()
		{
			if (pointers.Count != 3) return;

			// Cancel other gestures
			for (int i = 0; i < pointers.Count; i++)
			{
				if (pointers[i].TryHardCapture(this, DoNothing))
					pointers[i].ReleaseCapture(this);
			}

			if (!Children.Contains(_developerMenu))
			{
				Children.Insert(0, _developerMenu);
			}
		}

		void DoNothing() { }

		public void OnPointerMoved(PointerMovedArgs args)
		{
			//SetState(_state.OnPointerPressed(args));
			//if (!args.Handled)
				_defaultPointerEventResponder.OnPointerMoved(args);
		}

		public void OnPointerReleased(PointerReleasedArgs args)
		{
			for (int i = 0; i < pointers.Count; i++)
				if (pointers[i].PointIndex == args.PointIndex)
				{
					pointers.RemoveAt(i);
					break;
				}

			//SetState(_state.OnPointerPressed(args));
			//if (!args.Handled)
				_defaultPointerEventResponder.OnPointerReleased(args);
		}

		public void OnPointerWheelMoved(PointerWheelMovedArgs args)
		{
			//SetState(_state.OnPointerPressed(args));
			//if (!args.Handled)
				_defaultPointerEventResponder.OnPointerWheelMoved(args);
		}


		ConcurrentQueue<Exception> _exception = new ConcurrentQueue<Exception>();
 
		void OnUnhandledException(object sender, UnhandledExceptionArgs args)
		{
			args.IsHandled = true;
			_exception.Enqueue(args.Exception);
		}

		protected override void OnUpdate()
		{
			Exception exception = null, t;
			//drop all but last one
			while (_exception.TryDequeue(out t))
				exception = t;
			if (exception != null)
			{
				//A quick fix, a more proper fix would be https://github.com/fusetools/Fuse/issues/2174
				if (_state is Faulted)
				{
					//debug_log exception.Message;
					//debug_log "Failure state within a failed state. Simply ignoring that.";
				}
				else
				{
					ResetEverything(false);
					SetState(_state.OnException(exception));
				}
			}
			
			Action action;
			while (_dispatcher.TryDequeue(out action))
				action();

			SetState(_state.OnUpdate());
			base.OnUpdate();
		}

		void SetState(State nextState)
		{
			while (nextState != _state)
			{
				_state.OnLeaveState();
				_state = nextState;
				nextState = _state.OnEnterState();
			}
		}
	}

	abstract class State
	{
		public virtual State OnEnterState()
		{
			return this;
		}

		public virtual void OnLeaveState()
		{
		}

		public virtual State OnUpdate()
		{
			return this;
		}

		public abstract State OnException(Exception e);
	}

}
