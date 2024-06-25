using System;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public class ElementContext
	{
		readonly IContext _context;
		readonly IProject _project;
		readonly IMessagingService _daemon;

		public ElementContext(IContext context, IProject project, IMessagingService daemon)
		{
			_context = context;
			_project = project;
			_daemon = daemon;
		}

		public Menu CreateMenu(IElement element)
		{
			return Menu.Item(
					name: Texts.SubMenu_Element_LocateInEditor,
					command: FocusEditorCommand.Create(_context, _project, _daemon),
					hotkey: HotKey.Create(ModifierKeys.Meta | ModifierKeys.Alt, Key.L))

				+ Menu.Separator

				// Edit class element we're currently not editing
				+ Menu.Item(
						name: element.UxClass().Select(n => "Edit " + n.Or("class")).AsText(),
						isDefault: true,
						action: async () => await _context.PushScope(element, element))
					.ShowWhen(element.UxClass().Select(n => n.HasValue)
						.And(_context.CurrentScope.IsSameAs(element).IsFalse()))

				// Edit base
				+ Menu.Item(
					name: element.Base.UxClass().Select(n => "Edit " + n.Or("class")).AsText(),
					isEnabled: element.Base.IsReadOnly.IsFalse(),
					action: async () => await _context.PushScope(element.Base, element.Base))

				+ Menu.Item(
					name: Texts.SubMenu_Element_Deselect,
					hotkey: HotKey.Create(ModifierKeys.Meta, Key.D),
					action: async () => await _context.Select(Element.Empty))

				+ Menu.Separator

				+ Menu.Item(
					name: Texts.SubMenu_Element_Remove,
					command: Command.Create(
						element.Parent
								.IsEmpty
								// Only allow removal of non-root elements for now
								// Removing a root element could mean
								.Select(isRoot => isRoot ?
									Optional.None<Action>() :
									(Action) (async () =>
										{
											await element.Cut();

											if (await _context.IsSelected(element).FirstAsync())
												await _context.Select(Element.Empty);
										}))));
		}
	}
}
