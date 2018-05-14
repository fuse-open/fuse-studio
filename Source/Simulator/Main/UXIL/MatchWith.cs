using System;
using System.Linq;
using Uno.UX.Markup;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;
using EventHandler = Uno.UX.Markup.UXIL.EventHandler;
using String = Uno.UX.Markup.String;

namespace Outracks.Simulator.UXIL
{
	using Bytecode;

	public static class MatchWithExtensions
	{
		public static T MatchWith<T>(
			this IEvent self,
			Func<IAttachedEvent, T> a1,
			Func<IRegularEvent, T> a2)
		{
			var t1 = self as IAttachedEvent; if (t1 != null) return a1(t1);
			var t2 = self as IRegularEvent; if (t2 != null) return a2(t2);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this IProperty self,
			Func<IAttachedProperty, T> a1,
			Func<IProperty, T> a2)
		{
			var t1 = self as IAttachedProperty; if (t1 != null) return a1(t1);
			var t2 = self; if (t2 != null) return a2(t2);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this Node self,
			Func<DocumentScope, T> a1,
			Func<ObjectNode, T> a2,
			Func<PropertyNode, T> a3,
			Func<DependencyNode, T> a4)
		{
			var t1 = self as DocumentScope; if (t1 != null) return a1(t1);
			var t2 = self as ObjectNode; if (t2 != null) return a2(t2);
			var t3 = self as PropertyNode; if (t3 != null) return a3(t3);
			var t4 = self as DependencyNode; if (t4 != null) return a4(t4);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this ObjectNode self,
			Func<BoxedValueNode, T> a1,
			Func<NewObjectNode, T> a2,
			Func<ResourceRefNode, T> a3,
			Func<NameTableNode, T> a4)
		{
			var t1 = self as BoxedValueNode; if (t1 != null) return a1(t1);
			var t2 = self as NewObjectNode; if (t2 != null) return a2(t2);
			var t3 = self as ResourceRefNode; if (t3 != null) return a3(t3);
			var t4 = self as NameTableNode; if (t4 != null) return a4(t4);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this DocumentScope self,
			Func<ClassNode, T> a1,
			Func<TemplateNode, T> a3)
		{
			var t1 = self as ClassNode; if (t1 != null) return a1(t1);
			var t3 = self as TemplateNode; if (t3 != null) return a3(t3);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this ValueSource self,
			Func<ReferenceSource, T> a1,
			Func<AtomicValueSource, T> a2)
		{
			var t1 = self as ReferenceSource; if (t1 != null) return a1(t1);
			var t2 = self as AtomicValueSource; if (t2 != null) return a2(t2);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this ReferenceSource self,
			Func<BundleFileSource, T> a1,
			Func<NodeSource, T> a2,
			Func<UXPropertySource, T> a3,
			Func<UXPropertyAccessorSource, T> a4)
		{
			var t1 = self as BundleFileSource; if (t1 != null) return a1(t1);
			var t2 = self as NodeSource; if (t2 != null) return a2(t2);
			var t3 = self as UXPropertySource; if (t3 != null) return a3(t3);
			var t4 = self as UXPropertyAccessorSource; if (t4 != null) return a4(t4);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this AtomicValue self,
			Func<Bool, T> a1,
			Func<EnumValue, T> a2,
			Func<ReferenceValue, T> a3,
			Func<String, T> a4,
			Func<Scalar, T> a6,
			Func<Vector, T> a5,
			Func<GlobalReferenceValue, T> a7,
			Func<Uno.UX.Markup.Size, T> a8,
			Func<Size2, T> a9,
			Func<Selector, T> a10)
		{
			var t1 = self as Bool; if (t1 != null) return a1(t1);
			var t2 = self as EnumValue; if (t2 != null) return a2(t2);
			var t3 = self as ReferenceValue; if (t3 != null) return a3(t3);
			var t4 = self as String; if (t4 != null) return a4(t4);
			var t6 = self as Scalar; if (t6 != null) return a6(t6);
			var t7 = self as GlobalReferenceValue; if (t7 != null) return a7(t7);
			var t8 = self as Uno.UX.Markup.Size; if (t8 != null) return a8(t8);
			var t9 = self as Size2; if (t9 != null) return a9(t9);
			var t10 = self as Selector; if (t10 != null) return a10(t10);
			return a5(new Vector(self));
		}

		public static T MatchWith<T>(
			this Property self,
			Func<BindableProperty, T> a1,
			Func<AtomicProperty, T> a2,
			Func<DelegateProperty, T> a3)
		{
			var t1 = self as BindableProperty; if (t1 != null) return a1(t1);
			var t2 = self as AtomicProperty; if (t2 != null) return a2(t2);
			var t3 = self as DelegateProperty; if (t3 != null) return a3(t3);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this BindableProperty self,
			Func<ListProperty, T> a1,
			Func<ReferenceProperty, T> a2)
		{
			var t1 = self as ListProperty; if (t1 != null) return a1(t1);
			var t2 = self as ReferenceProperty; if (t2 != null) return a2(t2);
			throw new ArgumentException();
		}

		public static T MatchWith<T>(
			this EventHandler self,
			Func<EventMethod, T> a1,
			Func<EventBinding, T> a2)
		{
			var t1 = self as EventMethod; if (t1 != null) return a1(t1);
			var t2 = self as EventBinding; if (t2 != null) return a2(t2);
			throw new ArgumentException();
		}
	}

	public class Vector
	{
		readonly object _self;
		public Vector(object self)
		{
			_self = self;

			TypeName = TypeName.Parse(
				MatchWith(
					(Uno.UX.Markup.Vector<int> v) => "Uno.Int" + v.ComponentCount,
					(Uno.UX.Markup.Vector<uint> v) => "Uno.UInt" + v.ComponentCount,
					(Uno.UX.Markup.Vector<short> v) => "Uno.Short" + v.ComponentCount,
					(Uno.UX.Markup.Vector<byte> v) => "Uno.Byte" + v.ComponentCount,
					(Uno.UX.Markup.Vector<ushort> v) => "Uno.UShort" + v.ComponentCount,
					(Uno.UX.Markup.Vector<sbyte> v) => "Uno.Sbyte" + v.ComponentCount,
					(Uno.UX.Markup.Vector<float> v) => "Uno.Float" + v.ComponentCount,
					(Uno.UX.Markup.Vector<double> v) => "Uno.Double" + v.ComponentCount));

			Components = MatchWith(
				(Uno.UX.Markup.Vector<int> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<uint> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<short> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<byte> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<ushort> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<sbyte> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<float> v) => v.Components.Cast<Scalar>().ToArray(),
				(Uno.UX.Markup.Vector<double> v) => v.Components.Cast<Scalar>().ToArray());
		}

		public readonly TypeName TypeName;
		public readonly Scalar[] Components;

		public T MatchWith<T>(
			Func<Uno.UX.Markup.Vector<int>, T> a1,
			Func<Uno.UX.Markup.Vector<uint>, T> a2,
			Func<Uno.UX.Markup.Vector<short>, T> a3,
			Func<Uno.UX.Markup.Vector<byte>, T> a4,
			Func<Uno.UX.Markup.Vector<ushort>, T> a5,
			Func<Uno.UX.Markup.Vector<sbyte>, T> a6,
			Func<Uno.UX.Markup.Vector<float>, T> a7,
			Func<Uno.UX.Markup.Vector<double>, T> a8)
		{
			var t1 = _self as global::Uno.UX.Markup.Vector<int>; if (t1 != null) return a1(t1);
			var t2 = _self as global::Uno.UX.Markup.Vector<uint>; if (t2 != null) return a2(t2);
			var t3 = _self as global::Uno.UX.Markup.Vector<short>; if (t3 != null) return a3(t3);
			var t4 = _self as global::Uno.UX.Markup.Vector<byte>; if (t4 != null) return a4(t4);
			var t5 = _self as global::Uno.UX.Markup.Vector<ushort>; if (t5 != null) return a5(t5);
			var t6 = _self as global::Uno.UX.Markup.Vector<sbyte>; if (t6 != null) return a6(t6);
			var t7 = _self as global::Uno.UX.Markup.Vector<float>; if (t7 != null) return a7(t7);
			var t8 = _self as global::Uno.UX.Markup.Vector<double>; if (t8 != null) return a8(t8);
			throw new ArgumentException();
		}
	}
}