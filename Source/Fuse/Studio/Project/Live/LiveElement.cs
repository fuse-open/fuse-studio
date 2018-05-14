using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Xml.Linq;

namespace Outracks.Fuse.Live
{
	using Fusion;
	using IO;
	using Simulator;
	using Simulator.Protocol;

	partial class LiveElement : ILiveElement
	{
		readonly Optional<LiveElement> _parent;

		readonly BehaviorSubject<XElement> _element;

		XElement Element
		{
			get { return _element.Value; }
			set { _element.OnNext(value); }
		}

		readonly BehaviorSubject<SourceReference> _textPosition;
		readonly BehaviorSubject<ObjectIdentifier> _elementId =
			new BehaviorSubject<ObjectIdentifier>(new ObjectIdentifier("N/A", 0));

		readonly ConcurrentDictionary<string, IBehaviorProperty<Optional<string>>> _properties =
			new ConcurrentDictionary<string, IBehaviorProperty<Optional<string>>>();

		readonly BehaviorSubject<IImmutableList<LiveElement>> _children =
			new BehaviorSubject<IImmutableList<LiveElement>>(System.Collections.Immutable.ImmutableList<LiveElement>.Empty);

		readonly IObservable<ILookup<ObjectIdentifier, ObjectIdentifier>> _metadata;
		readonly IObservable<bool> _isReadOnly;

		readonly ISubject<Unit> _invalidated;
		readonly IObserver<IBinaryMessage> _mutations;
		readonly Func<ObjectIdentifier, IElement> _getElement;

		public LiveElement(
			AbsoluteFilePath file,
			IObservable<ILookup<ObjectIdentifier, ObjectIdentifier>> metadata,
			IObservable<bool> isReadOnly, 
			ISubject<Unit> invalidated, 
			IObserver<IBinaryMessage> mutations,
			Func<ObjectIdentifier, IElement> getElement) : this()
		{
			_parent = Optional.None();
			_metadata = metadata;
			_isReadOnly = isReadOnly;
			_invalidated = invalidated;
			_mutations = mutations;
			_getElement = getElement;

			_textPosition = new BehaviorSubject<SourceReference>(new SourceReference(file.NativePath, Optional.None()));

			Init();
		}

		public LiveElement(LiveElement parent) : this()
		{
			_parent = parent;
			_invalidated = parent._invalidated;
			_mutations = parent._mutations;
			_metadata = parent._metadata;
			_isReadOnly = parent._isReadOnly;
			_getElement = parent._getElement;
			
			_textPosition = new BehaviorSubject<SourceReference>(new SourceReference(parent._textPosition.Value.File, Optional.None()));

			Init();
		}

		LiveElement()
		{
			var xElement = new XElement("Unknown");
			xElement.AddAnnotation(this);
			_element = new BehaviorSubject<XElement>(xElement);
		}

		public void Init()
		{
			Name = CreateNameProperty(Element.Name.LocalName);

			SourceReference = _textPosition
				.Select(Optional.Some)
				.DistinctUntilChanged()
				.Replay(1).RefCount();

			Base = _metadata.Select(m => _getElement(m[_elementId.Value].FirstOr(ObjectIdentifier.None))).Switch();
		}
		
		public IObservable<bool> IsReadOnly
		{
			get { return _isReadOnly; }
		}

		IObservable<Optional<ILiveElement>> IElement.LiveElement { get { return Observable.Return(Optional.Some<ILiveElement>(this)); } }

		public IObservable<bool> IsEmpty
		{
			get { return Observable.Return(false); }
		}

		public IObservable<Optional<SourceReference>> SourceReference { get; private set; }

		public IBehavior<ObjectIdentifier> SimulatorId
		{
			get { return _elementId.AsBehavior(); }
		}
		IObservable<ObjectIdentifier> IElement.SimulatorId {  get { return _elementId; } }

		public IElement Base { get; private set; }

		public IElement Parent
		{
			get
			{
				return _parent.HasValue 
					? _parent.Value
					: Fuse.Element.Empty;
			}
		}

		public IBehaviorProperty<string> Name { get; private set; }

		IProperty<string> IElement.Name { get { return Name; } }

		public IObservable<IEnumerable<IElement>> Children { get { return _children; }}

		public IProperty<Optional<string>> Content 
		{ 
			get
			{
				return _properties.GetOrAdd("__Value__", _ =>
					CreateProperty(
						initialValue: Optional.None<string>(),
						isReadOnly: _children.Select(c => c.Count > 0),
						onSet: value =>
						{
							if (value.HasValue)
								Element.Value = value.Value;
							else
								Element.RemoveNodes();

							var id = _elementId.Value;
							if (!id.Equals(ObjectIdentifier.None))
							{
								_invalidated.OnNext(Unit.Default);
								_mutations.OnNext(new UpdateAttribute(id, "Value", value, _textPosition.Value));
							}
						}));
			} 
		}

		public IBehaviorProperty<Optional<string>> this[string propertyName]
		{
			get
			{
				return _properties.GetOrAdd(propertyName, _ => 
					CreateProperty(KeyToName(propertyName)));
			}
		}

		IProperty<Optional<string>> IElement.this[string propertyName]
		{
			get { return this[propertyName]; }
		}

		IBehaviorProperty<string> CreateNameProperty(string initialName)
		{
			return CreateProperty(
				initialValue: initialName,
				onSet: value =>
				{
					try
					{
						Element.Name = value;
					}
					catch (Exception)
					{
					}
					_invalidated.OnNext(Unit.Default);
					_mutations.OnNext(new ReifyRequired());
				});
		}

		IBehaviorProperty<Optional<string>> CreateProperty(XName name)
		{
			var key = NameToKey(name);

			return CreateProperty(
				initialValue: Optional.None<string>(),
				onSet: value =>
				{
					Element.SetAttributeValue(name, value.OrDefault());

					var id = _elementId.Value;
					if (!id.Equals(ObjectIdentifier.None))
					{
						_invalidated.OnNext(Unit.Default);
						_mutations.OnNext(new UpdateAttribute(id, key, value, _textPosition.Value));
					}
				});
		}

		IBehaviorProperty<T> CreateProperty<T>(T initialValue, Action<T> onSet, IObservable<bool> isReadOnly = null)
		{
			isReadOnly = isReadOnly ?? Observable.Return(false);

			var subject = new BehaviorSubject<T>(initialValue);

			var property = BehaviorProperty.Create(subject, _invalidated.Update(Unit.Default), isReadOnly);

			property.Skip(1)
				.DistinctUntilChanged()
				.Do(onSet)
				.Subscribe();

			return property;
		}

		static string NameToKey(XName name)
		{
			return (name.Namespace == XNamespace.Get("http://schemas.fusetools.com/ux") ? "ux:" : "") + name.LocalName;
		}

		static XName KeyToName(string name)
		{
			return name.StartsWith("ux:")
				? XName.Get(name.StripPrefix("ux:"), "http://schemas.fusetools.com/ux")
				: XName.Get(name);
		}

		public IObservable<Unit> Changed { get { return _invalidated.Merge(_element.Select(_ => Unit.Default)); }}

		IBehavior<IReadOnlyList<ILiveElement>> ILiveElement.Children
		{
			get { return _children.AsBehavior(); }
		}
	}
}
