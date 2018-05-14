using System;

namespace Outracks
{
	public interface IProperty<T> : IObservable<T>
	{
		IObservable<bool> IsReadOnly { get; }

		/// <summary>
		/// Write value to the source of this property. 
		/// The save flag indicates that the user is done editing this field for now (e.g. at the end of scrubbing a value), 
		/// so the source may perform heavier tasks like saving to disk in response.
		/// The source may also just ignore the save flag in many cases.
		/// </summary>
		void Write(T value, bool save = false);
	}
}