using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AppKit;

namespace Outracks.Fusion.OSX
{
	class ButtonImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Button.Implementation.Factory = (command, contentFactory, text, isDefault) =>
			{
				if (contentFactory != null)
				{
					var states = new BehaviorSubject<ButtonStates>(ButtonStates.Unrooted);

					var content = contentFactory(states.Switch());

					var hittableContent = content.MakeHittable();

					states.OnNext(new ButtonStates(
						isPressed: hittableContent.IsPressed(),
						isHovered: hittableContent.IsHovering(),
						isEnabled: command.IsEnabled));

					hittableContent.Pressed.WithLatestFromBuffered(command.Execute, (evt, c) => c)
						.ConnectWhile(content.IsRooted)
						.Subscribe(action => dispatcher.Schedule(action));

					return hittableContent.Control;
				}
				else
				{
					var width = new ReplaySubject<Points>(1);

					return Control.Create(self =>
					{
						Action action = () => { };

						
						var b = new ObservableButton()
						{
							BezelStyle = NSBezelStyle.Rounded,
						};

						b.Activated += (s, a) => action();

						if (isDefault)
						{
							b.WindowObservable
								.Where(window => window != null)
								.Subscribe(window => 
									Fusion.Application.MainThread.InvokeAsync(() => window.DefaultButtonCell = b.Cell));
						}

						command.Action
							.CombineLatest(text)
							.Take(1)
							.ObserveOn(dispatcher)
							.Subscribe(
								cmdText =>
								{
									UpdateButtonFields(b, cmdText.Item1.HasValue, cmdText.Item2, width);
								});

						self.BindNativeProperty(
							dispatcher,
							"command",
							command.Action.CombineLatest(text),
							cmdText =>
							{
								UpdateButtonFields(b, cmdText.Item1.HasValue, cmdText.Item2, width);
								action = () => cmdText.Item1.Do(x => x());
							});

						self.BindNativeDefaults(b, dispatcher);

						return b;
					})
					.WithHeight(32)
					.WithWidth(width);
				}
			};
		}

		static void UpdateButtonFields(ObservableButton b, bool enabled, string text, IObserver<Points> width)
		{
			b.Enabled = enabled;
			b.Title = text;
			b.Font = NSFont.SystemFontOfSize(0.0f);
			width.OnNext((double)b.FittingSize.Width);			
		}
	}

	class ObservableButton : NSButton
	{
		public ReplaySubject<NSWindow> WindowObservable = new ReplaySubject<NSWindow>(1);

		public ObservableButton()
		{			
		}

		public ObservableButton(IntPtr handle) : base(handle)
		{			
		}
		
		public override void ViewDidMoveToWindow()
		{
			WindowObservable.OnNext(Window);
		}
	}
}