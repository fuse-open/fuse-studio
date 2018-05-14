using System.Reactive.Subjects;

namespace Outracks
{
	public interface IBehaviorSubject<T> : IBehavior<T>, ISubject<T>
	{
	}
}