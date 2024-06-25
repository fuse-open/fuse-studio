using Uno.Compiler.ExportTargetInterop;

namespace Fuse.Simulator
{
	extern(!MOBILE) static class ScreenIdle
	{
		public static void Enable() {}
		public static void Disable() {}
	}

	extern(Android) static class ScreenIdle
	{
		[Foreign(Language.Java)]
		public static void Enable()
		@{
			com.fuse.Activity.getRootActivity().getWindow().clearFlags(
				android.view.WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		@}

		[Foreign(Language.Java)]
		public static void Disable()
		@{
			com.fuse.Activity.getRootActivity().getWindow().addFlags(
				android.view.WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		@}
	}

	[ForeignInclude(Language.ObjC, "UIKit/UIKit.h")]
	extern(iOS) static class ScreenIdle
	{
		[Foreign(Language.ObjC)]
		public static void Enable()
		@{
			[[UIApplication sharedApplication] setIdleTimerDisabled:NO];
		@}

		[Foreign(Language.ObjC)]
		public static void Disable()
		@{
			[[UIApplication sharedApplication] setIdleTimerDisabled:YES];
		@}
	}

	enum ScreenOrientationType
	{
		Portrait = 0,
		Landscape = 1,
		Sensor = 2
	}

	extern(!MOBILE) static class ScreenOrientation
	{
		public static void Request(ScreenOrientationType orientation)
		{
		}
	}

	extern(Android) static class ScreenOrientation
	{
		[Foreign(Language.Java)]
		public static void Request(ScreenOrientationType orientation)
		@{
			int orient = 0;
			if(orientation == 0)
				orient = android.content.pm.ActivityInfo.SCREEN_ORIENTATION_PORTRAIT;
			else if(orientation == 1)
				orient = android.content.pm.ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE;
			else if(orientation == 2)
				orient = android.content.pm.ActivityInfo.SCREEN_ORIENTATION_SENSOR;

			com.fuse.Activity.getRootActivity().setRequestedOrientation(orient);
		@}
	}

	[ForeignInclude(Language.ObjC, "UIKit/UIKit.h")]
	extern(iOS) static class ScreenOrientation
	{
		[Foreign(Language.ObjC)]
		public static void Request(ScreenOrientationType orientation)
		@{
		@}
	}
}