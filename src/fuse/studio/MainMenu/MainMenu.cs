using Outracks.Diagnostics;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Setup;
using Outracks.Fuse.Stage;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Studio
{
	static class MainMenu
	{
		public static Menu Create(
			IFuse fuse,
			IMessagingService daemon,
			IShell shell,
			StageController stage,
			Help help,
			Menu elementMenu,
			Menu projectMenu,
			Build preview,
			Export export,
			SetupGuide setupGuide,
			CheckForUpdates checkForUpdates,
			Menu windowMenu,
			Debug debug,
			Activate activate)
		{
			var about = new About(fuse.Version, fuse.License, daemon, debug, activate);

			var menus
				= Menu.Submenu(Texts.MainMenu_File, CreateFileMenu())
				+ Menu.Submenu(Texts.MainMenu_Edit, Application.EditMenu)
				+ Menu.Submenu(Texts.MainMenu_Element, elementMenu)
				+ Menu.Submenu(Texts.MainMenu_Project, projectMenu)
				+ Menu.Submenu(Texts.MainMenu_Viewport, stage?.Menu ?? new Menu())
				+ Menu.Submenu(Texts.MainMenu_Preview, preview?.Menu ?? new Menu())
				+ Menu.Submenu(Texts.MainMenu_Export, export?.Menu ?? new Menu())
				+ Menu.Submenu(Texts.MainMenu_Tools, setupGuide.Menu)
				+ Menu.Submenu(Texts.MainMenu_Window, windowMenu)
				+ Menu.Submenu(Texts.MainMenu_Help, CreateHelpMenu(help, checkForUpdates, about, activate))
				+ (debug?.Menu ?? new Menu());

			if (Platform.IsMac)
				return Menu.Submenu("fuse X", CreateFuseMenu(checkForUpdates, about, activate))
					 + menus;

			return menus;
		}

		static Menu CreateHelpMenu(Help help, CheckForUpdates checkForUpdates, About about, Activate activate)
		{
			return (Platform.IsWindows ? help.Menu + Menu.Separator + checkForUpdates.Menu + Menu.Separator + activate.Menu + Menu.Separator + about.Menu : help.Menu);
		}

		static Menu CreateFuseMenu(CheckForUpdates checkForUpdates, About about, Activate activate)
		{
			return about.Menu
				 + Menu.Separator
				 + activate.Menu
				 + Menu.Separator
				 + checkForUpdates.Menu
				 + Menu.Separator
				 + CreateQuitItem();
		}

		static Menu CreateFileMenu()
		{
			return CreateSplashScreenItem()
				+ CreateOpenItem()
				+ (Platform.IsWindows
					? Menu.Separator + CreateQuitItem()
					: Menu.Empty);
		}

		static Menu CreateSplashScreenItem()
		{
			return Menu.Item(
				Texts.SubMenu_File_New,
				hotkey: HotKey.Create(ModifierKeys.Meta, Key.N),
				action: () => Application.LaunchedWithoutDocuments());
		}

		static Menu CreateOpenItem()
		{
			return Menu.Item(
				Texts.SubMenu_File_Open,
				hotkey: HotKey.Create(ModifierKeys.Meta, Key.O),
				action: () => Application.ShowOpenDocumentDialog(new FileFilter("fuse X Project", "unoproj")));
		}

		static Menu CreateQuitItem()
		{
			var quitItem = Menu.Item(
				Platform.IsMac ? Texts.SubMenu_File_Quit : Texts.SubMenu_File_Exit,
				hotkey: Platform.IsWindows ? HotKey.Create(ModifierKeys.Alt, Key.F4) : HotKey.Create(ModifierKeys.Command, Key.Q),
				action: () => { Application.Exit(0); });
			return quitItem;
		}
	}
}
