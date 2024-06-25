using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class PropertyTests
	{
		[Test]
		public async Task Switch_remembers_set_value()
		{
			var obs = Observable
				.Return(true)
				.Select(_ => Property.Create("initial"))
				.Switch();

			obs.Write("second");
			Assert.That(await obs.FirstAsync(), Is.EqualTo("second"));
			Assert.That(await obs.FirstAsync(), Is.EqualTo("second"));

			obs.Write("third");
			Assert.That(await obs.FirstAsync(), Is.EqualTo("third"));
		}

		[Test]
		public void IProperty_Deferred_doesnt_defer_inner_Set()
		{
			var value = "initial";
			var prop = MakeProperty(value, t => value = t);

			var deferredProp = prop.Deferred();

			var deferredValue = "";

			using (deferredProp.Subscribe(newValue => deferredValue = newValue))
			{
				prop.Write("first");
				prop.Write("second");

				Assert.That(value, Is.EqualTo("second"));
				Assert.That(deferredValue, Is.EqualTo("second"));
			}
		}

		[Test]
		public void IProperty_Deferred_defers_Set()
		{
			var value = "initial";
			var prop = MakeProperty(value, t => value = t);

			var deferredProp = prop.Deferred();

			var deferredValue = "";

			using (deferredProp.Subscribe(newValue => deferredValue = newValue))
			{
				deferredProp.Write("first");
				deferredProp.Write("second");

				Assert.That(value, Is.EqualTo("initial"));
				Assert.That(deferredValue, Is.EqualTo("second"));

				deferredProp.Write("second", save: true);

				Assert.That(value, Is.EqualTo("second"));
				Assert.That(deferredValue, Is.EqualTo("second"));
			}
		}

		[Test]
		public void IProperty_With_defaults_to_source_invalidate_command()
		{
			var sourceInvalidateCalled = false;

			var sourceProp = Observable.Return(42).AsProperty((v, save) => sourceInvalidateCalled |= save);

			sourceProp.With().Write(1337, save: true);
			Assert.That(sourceInvalidateCalled, Is.True);
		}

		static IProperty<T> MakeProperty<T>(T value, Action<T> onSet)
		{
			var prop = Property.Create(value);
			prop.Subscribe(onSet);
			return prop;
		}
	}
}
