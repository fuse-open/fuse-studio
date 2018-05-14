using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public interface IValidatedProperty<T> : IProperty<T>
	{
		IObservable<Optional<string>> ValidationError { get; }
	}

	public static class ValidatedProperty
	{
		public static IValidatedProperty<string> Validate<T>(
			this IProperty<Optional<T>> self,
			Func<T, string> toString,
			Func<string, IValidationResult<T>> fromString)
		{
			var validationError = new BehaviorSubject<Optional<string>>(Optional.None());
			return new ValidatedProperty<string>
			{
				Value = self.SelectPerElement(toString).Or(""),
				IsReadOnly = self.IsReadOnly,
				ValidationError = validationError.Merge(self.Select(v => Optional.None<string>())),
				Writer = (v, save) => fromString(v).MatchWith(
					(T t) => 
					{
						self.Write(Optional.Some(t));
						validationError.OnNext(Optional.None<string>());
						return false;
					},
					(string e) => 
					{
						self.Write(Optional.None());
						validationError.OnNext(Optional.Some(e));
						return false;
					}),
			};
		}
	}

	class ValidatedProperty<T> : IValidatedProperty<T>
	{
		public IObservable<T> Value { get; set; }
		public IObservable<bool> IsReadOnly { get; set; }
		public IObservable<Optional<string>> ValidationError { get; set; }
		public Action<T, bool> Writer { get; set; }
		
		public void Write(T value, bool save)
		{
			Writer(value, save);
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return Value.Subscribe(observer);
		}
	}
}
