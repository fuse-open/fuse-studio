using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public interface IModalHost
	{
		void OpenModal(Func<Command, IControl> show);
	}

	public static class Modal
	{
		public static IControl Host(Func<IModalHost, IControl> content)
		{
			var modalHost = new Implementation();
			return content(modalHost).WithNativeOverlay(modalHost.OverlayControls.Layer());
		}

		class Implementation : IModalHost
		{
			readonly BehaviorSubject<IEnumerable<IControl>> overlayControls = new BehaviorSubject<IEnumerable<IControl>>(ImmutableList<IControl>.Empty);

			public IObservable<IEnumerable<IControl>> OverlayControls
			{
				get { return overlayControls; }
			}

			public void OpenModal(Func<Command, IControl> show)
			{
				IControl modalContent = null;
				var closeCommand = Command.Enabled(
					() =>
					{
						if (modalContent != null)
						{
							this.overlayControls.OnNext(new IControl[] {});
						}
					});
				modalContent = show(closeCommand);
				this.overlayControls.OnNext(new [] { modalContent });
			}
		}
	}
}