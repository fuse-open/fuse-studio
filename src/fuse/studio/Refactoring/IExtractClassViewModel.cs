using System;
using Outracks.Fusion;

namespace Outracks.Fuse.Refactoring
{
	public interface IExtractClassViewModel
	{
		/// <summary>
		/// Command to be invoked when the user clicks "Create class" button
		/// </summary>
		Command CreateCommand { get; }

		/// <summary>
		/// The resulting class name, for binding to a textbox.
		/// A default suggested value is provided.
		/// </summary>
		IProperty<string> ClassName { get; }

		/// <summary>
		/// The name of the new file, for binding to a textbox with a "browse.." button on the left side
		/// Only enabled while CreateInNewFile is true.
		/// </summary>
		IProperty<string> NewFileName { get; }

		/// <summary>
		/// Controls hether to put the newly created class in a new file or not.
		/// </summary>
		IProperty<bool> CreateInNewFile { get; }

		/// <summary>
		/// A string that provides hints to the user. For example when something goes wrong.
		/// </summary>
		IObservable<string> UserInfo { get; }
	}
}