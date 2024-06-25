using System;
using Uno;
using Uno.Compiler.ExportTargetInterop;
using Uno.IO;
using Uno.Net;
using Uno.Collections;
using Fuse;
using Uno.Diagnostics;
using Fuse.Controls;
using Fuse.Elements;

namespace Outracks.Simulator.Client
{
	[ForeignInclude(Language.Java, "android.os.Build")]
	[ForeignInclude(Language.ObjC, "UIKit/UIKit.h")]
	public extern(Android || iOS) static class DeviceInfo
	{
		public static string Name { get { return GetName(); }}

		public static string GUID { get { return GetGUID(); }}

		[Foreign(Language.ObjC)]
		public static extern(iOS) string GetName()
		@{
			return [[UIDevice currentDevice] name];
		@}

		[Foreign(Language.ObjC)]
		public static extern(iOS) string GetGUID()
		@{
			return [[[UIDevice currentDevice] identifierForVendor] UUIDString];
		@}

		[Foreign(Language.Java)]
		public static extern(Android) string GetName()
		@{
			return Build.MODEL;
		@}
		
		[Foreign(Language.Java)]
		public static extern(Android) string GetGUID()
		@{
			return Build.SERIAL;
		@}
	}

	[DotNetType("System.Guid")]
	extern(DotNet) public struct Guid
	{
		private extern int _foo;
		public static extern Guid NewGuid();
		public extern override string ToString();
	}
	
	public extern(DotNet) static class DeviceInfo
	{
		/******************************** NOTE *****************************************
		 * Before changing the value of 'Name' make sure to find all references to the string 'Viewport',
		 * since there are code out there depending on that specific string!
		 *******************************************************************************/
		public static readonly string Name = "Viewport";
		public static readonly string GUID;

		static DeviceInfo()
		{
			GUID = Guid.NewGuid().ToString();
		}		
	}

	public extern(!(DotNet || Android || iOS)) static class DeviceInfo
	{
		public static readonly string Name = "";
		public static readonly string GUID = "00000000-0000-0000-0000-000000000000";
	}
}
