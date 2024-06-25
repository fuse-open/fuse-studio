using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Inspector
{
	public class AttributeIntercepter : IEditorFactory
	{
		class AttributeRecord
		{
			public IObservable<bool> HasValue;
			public IObservable<bool> IsReadOnly;
		}

		readonly BehaviorSubject<IImmutableSet<AttributeRecord>> _attributes = new BehaviorSubject<IImmutableSet<AttributeRecord>>(ImmutableHashSet<AttributeRecord>.Empty);

		readonly IEditorFactory _editorFactory;

		public AttributeIntercepter(IEditorFactory editorFactory)
		{
			_editorFactory = editorFactory;

			AllReadOnly = _attributes
				.Select(attributes =>
					attributes
						.Select(a => a.IsReadOnly)
						.Aggregate(Observable.Return(true), ObservableBooleanExtensions.And))
				.Switch()
				.Replay(1).RefCount();

			AnyHasValue = _attributes
				.Select(attributes =>
					attributes
						.Select(a => a.HasValue)
						.Aggregate(Observable.Return(false), ObservableBooleanExtensions.Or))
				.Switch()
				.Replay(1).RefCount();
		}

		public IObservable<bool> AllReadOnly { get; private set; }
		public IObservable<bool> AnyHasValue { get; private set; }

		public IControl ElementList(Text name, IElement parent, SourceFragment prototype, Func<IElement, IControl> itemFactory)
		{
			return _editorFactory.ElementList(name, parent, prototype, itemFactory);
		}

		public IControl Label<T>(Text name, params IAttribute<T>[] properties)
		{
			var control = _editorFactory.Label(name, properties);

			foreach (var a in properties)
				Intercept(a, control);

			return control;
		}

		public IControl Label(Text name, IAttribute<Points> attribute)
		{
			return Intercept(attribute, _editorFactory.Label(name, attribute));
		}

		public IControl Label(Text name, IAttribute<UxSize> attribute)
		{
			return Intercept(attribute, _editorFactory.Label(name, attribute));
		}

		public IEditorControl Field<T>(IAttribute<T> attribute, Text placeholderText = default(Text), Text toolTip = default(Text), bool deferEdit = false)
		{
			return Intercept(attribute, _editorFactory.Field(attribute, placeholderText, toolTip, deferEdit));
		}

		public IControl Label(Text name, IProperty<Optional<string>> attributeData)
		{
			return Intercept(attributeData, _editorFactory.Label(name, attributeData));
		}

		public IEditorControl Slider(IAttribute<double> attribute, double min, double max)
		{
			return Intercept(attribute, _editorFactory.Slider(attribute, min, max));
		}

		public IEditorControl FilePath(IAttribute<string> attribute, IObservable<AbsoluteDirectoryPath> projectRoot, FileFilter[] fileFilters, Text placeholderText = default(Text), Text toolTip = default(Text))
		{
			return Intercept(attribute, _editorFactory.FilePath(attribute, projectRoot, fileFilters, placeholderText, toolTip));
		}

		public IEditorControl Switch(IAttribute<bool> attribute)
		{
			return Intercept(attribute, _editorFactory.Switch(attribute));
		}

		public IEditorControl Color(IAttribute<Color> color)
		{
			return Intercept(color, _editorFactory.Color(color));
		}

		public IEditorControl Dropdown<T>(IAttribute<T> attribute) where T : struct
		{
			return Intercept(attribute, _editorFactory.Dropdown(attribute));
		}

		public IRadioButton<T> RadioButton<T>(IAttribute<T> attribute)
		{
			var control = _editorFactory.RadioButton(attribute);
			Intercept(attribute, control.Control);
			return control;
		}

		public IControl ExpressionButton<T>(IAttribute<T> attribute)
		{
			return Intercept(attribute, _editorFactory.ExpressionButton(attribute));
		}

		public TResult Intercept<T, TResult>(IAttribute<T> attribute, TResult control)
			where TResult : IControl
		{
			return Keep(control, new AttributeRecord
			{
				HasValue = attribute.HasLocalValue(),
				IsReadOnly = attribute.IsReadOnly,
			});
		}

		public TResult Intercept<T, TResult>(IProperty<T> property, TResult control)
			where TResult : IControl
		{
			return Keep(control, new AttributeRecord
			{
				HasValue = Observable.Return(false),
				IsReadOnly = property.IsReadOnly,
			});
		}

		TResult Keep<TResult>(TResult control, AttributeRecord record)
			where TResult : IControl
		{
			control.IsRooted.Subscribe(rooted =>
				_attributes.OnNext(rooted
					? _attributes.Value.Add(record)
					: _attributes.Value.Remove(record)));

			return control;
		}
	}
}
