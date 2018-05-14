using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	class CustomButton : ButtonBase
	{
	}

	static class ButtonImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			
			Button.Implementation.Factory = (command, contentFactory, text, isDefault) =>
			{
				if (contentFactory != null)
				{
					var states = new BehaviorSubject<ButtonStates>(ButtonStates.Unrooted);

					var content = contentFactory(states.Switch());

					return Control.Create(self =>
					{
						Action action = () => { };
						var button = new CustomButton();
							button.Click += (s, a) => action();

						states.OnNext(new ButtonStates(
							isPressed: button.ObserveDependencyProperty(instance => instance.IsPressed, ButtonBase.IsPressedProperty),
							isHovered: button.ObserveDependencyProperty(instance => instance.IsMouseOver, ButtonBase.IsMouseOverProperty),
							isEnabled: button.ObserveDependencyProperty(instance => instance.IsEnabled, ButtonBase.IsEnabledProperty)));

						content.Mount(
							new MountLocation.Mutable
							{
								NativeFrame = ObservableMath.RectangleWithSize(self.NativeFrame.Size),
								AvailableSize = self.AvailableSize,
								IsRooted = self.IsRooted,
							});

						var child = content.NativeHandle as UIElement;
						if (child != null)
						{
							var grid = new Grid();
							grid.Children.Add(
								new System.Windows.Shapes.Rectangle()
								{
									Fill = new SolidColorBrush(Colors.Transparent)
								});
							grid.Children.Add(child);
							button.Content = grid;
						};

						
						self.BindNativeProperty(dispatcher, "command", command.Action, cmd =>
						{
							button.IsEnabled = cmd.HasValue;
							action = () => cmd.Do(x => x());
						});

						self.BindNativeDefaults(button, dispatcher);

						return button;
					})
					.WithSize(content.DesiredSize);
				}
				else
				{
					var width = new ReplaySubject<Points>(1);

					return Control.Create(self =>
					{
						Action action = () => { };
						var button = new System.Windows.Controls.Button();
						button.Click += (s, a) => action();
				

						command
							.Action
							.CombineLatest(text)
							.Take(1)
							.ObserveOn(Fusion.Application.MainThread)
							.Subscribe(
								cmdText =>
								{
									UpdateButtonFields(button, cmdText.Item1.HasValue, cmdText.Item2, width);
								});

						self.BindNativeProperty(dispatcher, "command", command.Action.CombineLatest(text), cmdText =>
						{
							UpdateButtonFields(button, cmdText.Item1.HasValue, cmdText.Item2, width);
							action = () => cmdText.Item1.Do(x => x());
						});

						self.BindNativeDefaults(button, dispatcher);

						return button;
					})
					.WithHeight(20)
					.WithWidth(width)
					.WithPadding(new Thickness<Points>(6));
				}
			};
		}

		static void UpdateButtonFields(System.Windows.Controls.Button button, bool enabled, string text, IObserver<Points> width)
		{
			button.Content = text;
			button.IsEnabled = enabled;
			button.Measure(new System.Windows.Size(double.MaxValue, double.MaxValue));
			width.OnNext(button.DesiredSize.Width);
		}

		static IObservable<TOut> ObserveDependencyProperty<TIn, TOut>(
			this TIn instance,
			Func<TIn, TOut> getValue,
			DependencyProperty dep)
		{
			return Fusion.Application.MainThread
				.InvokeAsync(() =>
				{
					var observable = new BehaviorSubject<TOut>(getValue(instance));
					DependencyPropertyDescriptor.FromProperty(dep, typeof (TIn))
						.AddValueChanged(instance, (sender, args) => observable.OnNext(getValue(instance)));
					return observable;
				})
				.ToObservable()
				.Switch();
		}
	}
}