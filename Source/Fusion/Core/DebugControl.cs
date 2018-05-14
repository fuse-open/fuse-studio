using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Outracks.IO;

namespace Outracks.Fusion
{
	class DebugControl
	{
		public readonly string Name;
		public readonly IControl Control;
		public readonly Rectangle<IObservable<Points>> Frame;
		public readonly Size<IObservable<Points>> AvailableSize;
		public readonly AbsoluteFilePath FilePath;
		public readonly int LineNumber;

		public DebugControl(string name, IControl control, Rectangle<IObservable<Points>> frame, Size<IObservable<Points>> availableSize, AbsoluteFilePath filePath, int lineNumber)
		{
			Name = name;
			Control = control;
			Frame = frame;
			AvailableSize = availableSize;
			FilePath = filePath;
			LineNumber = lineNumber;
		}
	}

	public static class Debugging
	{
		static readonly BehaviorSubject<ImmutableDictionary<string, DebugControl>> DebugControls 
			= new BehaviorSubject<ImmutableDictionary<string, DebugControl>>(ImmutableDictionary<string, DebugControl>.Empty);
		static readonly BehaviorSubject<Optional<string>> SelectedControl = new BehaviorSubject<Optional<string>>(Optional.None());

		public static IControl Debug(this IControl ctrl, string name, [CallerFilePath] string filepath = "", [CallerLineNumber]int lineNumber = 0)
		{
#if DEBUG
			var location = new ReplaySubject<IMountLocation>(1);
			ctrl = ctrl.WithMountLocation(
				oldLocation =>
				{
					location.OnNext(oldLocation);
					return oldLocation;
				});

			var debugCtrl = new DebugControl(
				name, 
				ctrl, 
				location.Select(l => l.NativeFrame).Switch(), 
				location.Select(l => l.AvailableSize).Switch(),
				AbsoluteFilePath.Parse(filepath), 
				lineNumber);

			DebugControls.OnNext(DebugControls.Value.SetItem(debugCtrl.Name, debugCtrl));

			var selected = Command.Enabled(action: () => SelectedControl.OnNext(name));

			return ctrl.WithOverlay(
				SelectedControl.StartWithNone()
					.Select(selCtrl => selCtrl.MatchWith(n => n == name, () => false))
					.Select(isSelected => isSelected
						? Shapes.Rectangle(stroke: Stroke.Create(2, Color.FromBytes(0x6d, 0xc0, 0xd2)), fill: Color.FromBytes(0x6d, 0xc0, 0xd2, 50))
						: Shapes.Rectangle().OnMouse(pressed: selected))
					.Switch()
			);
#else
			return ctrl;
#endif
		}

		public static IControl CreateDebugControl()
		{
			var debugControls = DebugControls
				.Throttle(TimeSpan.FromMilliseconds(10));
			var controls = debugControls
				.CachePerElement(getKey: ctrl => ctrl.Value, getValue: ctrl => ctrl.Value.Name)
				.SelectPerElement(name =>
					CreateDebugItem(name, Command.Enabled(action: () => SelectedControl.OnNext(name))));

			var infoPanel = SelectedControl
				.CombineLatest(debugControls, (sel, ctrls) => sel.HasValue ? CreateInfoPanel(ctrls[sel.Value]) : CreateNothingSelected())
				.Switch();

			return Layout.Dock()
				.Left(
					Layout.Dock().Top(Header("Debug Controls"))
					.Fill(controls.StackFromTop().MakeScrollable())
					.WithWidth(200)
					.WithPadding(new Thickness<Points>(10))
				)
				.Left(
					Layout.StackFromTop(
						Header("Mount Info"),
						infoPanel
					).WithPadding(new Thickness<Points>(10))
				)
				.Fill()
				.WithPadding(new Thickness<Points>(10));
		}

		static IControl CreateDebugItem(string name, Command selected)
		{
			var isSelected = SelectedControl.NotNone().Select(sel => sel == name);
			return Button.Create(
				text: name, 
				clicked: selected,
				content: states =>
					Label.Create(name, color: states.IsHovered.Select(hovered => hovered ? Color.FromBytes(0x6d, 0xc0, 0xd2) : Color.White).AsBrush())
						.WithPadding(new Thickness<Points>(10, 5, 10, 5))
						.WithOverlay(Shapes.Rectangle(fill: isSelected.Select(hovered => hovered ? Color.FromBytes(0x6d, 0xc0, 0xd2, 100) : Color.Transparent).AsBrush()))
			)
			.SetContextMenu(Menu.Item("Open in Text Editor", () => new Shell().OpenWithDefaultApplication(DebugControls.Value[name].FilePath)));
		}

		static IControl CreateInfoPanel(DebugControl selCtrl)
		{
			var rooted = selCtrl.Control.IsRooted;
			var desiredSize = selCtrl.Control.DesiredSize.Transpose();
			var nativeFrame = selCtrl.Frame.Transpose();
			var availableSize = selCtrl.AvailableSize.Transpose();
			var actualSize = nativeFrame.Size();

			return Layout.StackFromTop(
				CreateProperty("IsRooted", rooted, r => r.ToString()),
				CreateProperty("Desired Size", desiredSize, d => d.Format()),
				CreateProperty("Actual Size", actualSize, a => a.Format()),
				CreateProperty("Available Size", availableSize, a => a.Format()),
				CreateProperty("Native Frame", nativeFrame, f => f.Format()).WithPadding(new Thickness<Points>(0, 20, 0, 0))
			)
			.WithPadding(new Thickness<Points>(0, 5, 0, 0));
		}

		static IControl CreateNothingSelected()
		{
			return Layout.StackFromTop(
				Label.Create("Nothing selected")
			)
			.WithPadding(new Thickness<Points>(0, 5, 0, 0));
		}

		static IControl Header(string header)
		{
			return Layout.StackFromTop(
				Label.Create(header, Font.SystemDefault(15)),
				Shapes.Rectangle(fill: Color.FromRgb(0xffffff)).WithHeight(1).WithPadding(new Thickness<Points>(0, 5, 0, 5))
			);
		}

		static IControl CreateProperty<T>(string propertyName, IObservable<T> value, Func<T, string> converter)
		{
			var prefix = propertyName + ": ";
			return Label.Create(
				value.StartWithNone()
					.Select(val => val.MatchWith(
						some: v => prefix + converter(v), 
						none: () => prefix + " - "))
					.AsText());
		}

		static string Format(this Rectangle<Points> rect)
		{
			return "\n{\n    Position: " + rect.Position.Format() + "\n    Size: " + rect.Size.Format() + "\n}";
		}

		static string Format(this Point<Points> pos)
		{
			return pos.X.ToDouble().ToString("F") + "pt, " + pos.Y.ToDouble().ToString("F") + "pt";
		}

		static string Format(this Size<Points> size)
		{
			return size.Width.ToDouble().ToString("F") + "pt x " + size.Height.ToDouble().ToString("F") + "pt";
		}
	}
}