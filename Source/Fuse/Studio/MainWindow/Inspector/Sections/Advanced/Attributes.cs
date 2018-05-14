using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	public class AttributesSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return CreateAttributeEditorsForType(element, element.Base, editors);
		}

		static IControl CreateAttributeEditorsForType(IElement instance, IElement type, IEditorFactory editors)
		{
			var attributes = type.Children
				.SelectPerElement(c => c.UxProperty().SelectPerElement(p => new { Name = p, Type = c.Name, }))
				.ToObservableEnumerable()
				.Select(v => v.NotNone());
				
			return type.UxClass()
				.Select(maybeClass => 
					maybeClass.MatchWith(
						none: () => Layout.StackFromTop(
							Spacer.Medim,
							
							Label.Create("Unknown element type", 
									font: Theme.DefaultFont, 
									color: Theme.DisabledText, 
									textAlignment: TextAlignment.Center)
								.WithInspectorPadding(),
		
							Spacer.Medium),
						
						some: className => Layout.StackFromTop(
							Separator.Weak,
							Spacer.Medim,

							Label.Create(className, 
									font: Theme.DefaultFont, 
									color: Theme.DefaultText, 
									textAlignment: TextAlignment.Center),
									
							Spacer.Medium,

							attributes.SelectPerElement(attribute => 
								Layout.Dock()
									.Right(editors.Field(instance.GetString(attribute.Name, ""), placeholderText: attribute.Type.AsText()))
									.Fill(editors.Label(attribute.Name, instance[attribute.Name])))
								.StackFromTop(separator: () => Spacer.Small)
								.WithInspectorPadding(),

							Label.Create("This class has no properties",
									font: Theme.DefaultFont,
									color: Theme.DisabledText,
									textAlignment: TextAlignment.Center)
								.WithInspectorPadding()
								.ShowWhen(attributes.Select(a => a.IsEmpty())),

							Spacer.Medium,

							// Recurse up the IElement.Base chain
							type.Base.IsEmpty
								.Select(baseIsEmpty => 
									baseIsEmpty 
										? Control.Empty
										: Layout.StackFromTop(
											Separator.Weak,
											CreateAttributeEditorsForType(instance, type.Base, editors)))
								.Switch())))
				.Switch();
		}
	}
}
