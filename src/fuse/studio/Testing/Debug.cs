using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Outracks.Fuse.Testing;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	public class Debug
	{
		const int EnableDebugMenuClickCount = 10;

		readonly IProject _project;
		readonly IProperty<Optional<bool>> _alwaysShowDebugMenuSetting;

		internal Debug(IProject project)
		{
			_project = project;
			_alwaysShowDebugMenuSetting = UserSettings.Bool("AlwaysShowDebugMenu");
		}

		public Menu Menu
		{
			get
			{
#if DEBUG
				return Menu.Submenu("Debug", Create());
#else
				return Menu.Submenu("Debug", Create()).ShowWhen(_alwaysShowDebugMenuSetting.Select(x => x.Or(false)));
#endif
			}
		}

		public Command EnableDebugMenuByRapidClicking
		{
			get
			{
				List<DateTime> enableDebugMenuClicks = new List<DateTime>(EnableDebugMenuClickCount);

				return
					_alwaysShowDebugMenuSetting
						.Select(configValue =>
							Command.Enabled(() => _alwaysShowDebugMenuSetting.Write(!configValue.Or(false), save: true)))
						.Select(c =>
							Command.Enabled(
								c.Execute.Select(
									setAction => (Action) (() =>
									{
										var now = DateTime.UtcNow;
										enableDebugMenuClicks.RemoveAll(x => x < now.AddSeconds(-4));
										enableDebugMenuClicks.Add(now);
										if (enableDebugMenuClicks.Count > 10)
										{
											enableDebugMenuClicks.Clear();
											setAction();
										}
									}))))
						.Switch();
			}
		}

		Menu Create()
		{
			var createDebugWindow = new Action(() =>
				Application.Desktop.CreateSingletonWindow(
					Observable.Return(true),
					dialog =>
						new Window()
						{
							Title = Observable.Return("Debug"),
							Size = Property.Create<Optional<Size<Points>>>(new Size<Points>(600, 500)).ToOptional(),
							Content = Control.Lazy(Debugging.CreateDebugControl),
						}
				));

			return
				  Menu.Item("Open Debug Window", action: createDebugWindow)
				+ Menu.Item("Random ux mutator", action: () => RandomMutatorWindow.Create(_project))
				+ Menu.Item("Console output", action: ConsoleOutputWindow.Create)
				+ Menu.Item("Icon viewer", action: IconPreviewWindow.Create);
		}
	}
}