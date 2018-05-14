using System;
using System.Threading.Tasks;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;

	class EditorControl<T> : IEditorControl
	{
		readonly IEditorFactory _editors;
		readonly IAttribute<T> _attribute;
		readonly IControl _control;

		public EditorControl(IEditorFactory editors, IAttribute<T> attribute, IControl control)
		{
			_editors = editors;
			_attribute = attribute;
			_control = control;
		}

		public Action<IMountLocation> Mount
		{
			get { return _control.Mount; }
		}
	
		public Size<IObservable<Points>> DesiredSize
		{
			get { return _control.DesiredSize; }
		}

		public object NativeHandle
		{
			get { return _control.NativeHandle; }
		}

		public IObservable<bool> IsRooted
		{
			get { return _control.IsRooted; }
		}

		public IControl WithLabelAbove(Text description)
		{
			return Layout.StackFromTop(
				_editors.Label(description, _attribute).DockBottom().WithHeight(20),
				_control);
		}
		public IControl WithLabel(Text description)
		{
			return Layout.Dock()
				.Right(_control)
				.Fill(_editors.Label(description, _attribute));
		}

		public IControl WithIcon(Text tooltip, IControl icon)
		{
			return Layout.Dock()
				.Left(tooltip.IsDefault ? icon : icon.MakeHittable().Control) // The icon has to be hittable for tooltips to work
				.Left(Spacer.Small)
				.Fill(_control.CenterVertically())
				.WithHeight(_control.DesiredSize.Height)
				.WithWidth(_control.DesiredSize.Width)
				.WithBackground(Color.AlmostTransparent)
				.SetToolTip(tooltip);
		}
	}
}