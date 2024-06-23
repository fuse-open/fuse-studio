using System.Reactive.Subjects;

namespace Outracks.Fuse
{
	static class BehaviorSubjectExtensions
	{
		public static void OnNextDistinct<T>(this BehaviorSubject<T> subject, T value)
		{
			if (!value.Equals(subject.Value))
				subject.OnNext(value);
		}
	}
}