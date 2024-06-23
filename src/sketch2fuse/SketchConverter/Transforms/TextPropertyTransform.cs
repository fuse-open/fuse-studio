using System;
using System.Collections.Generic;
using System.Linq;
using SketchConverter.API;
using SketchConverter.UxBuilder;
using SketchImporter.UxGenerator;

namespace SketchConverter.Transforms
{
	public class TextPropertyTransform : ITransform
	{
		private readonly ILogger _log;

		public TextPropertyTransform(ILogger log)
		{
			_log = log;
		}

		public void Apply(UxNode uxClass)
		{
			var propertyValues = new List<PropertyValue>();
			Apply(uxClass, propertyValues);
			for(var i = propertyValues.Count-1; i>=0; i--)
			{
                uxClass.Children.Insert(0,StringProperty(propertyValues[i].Name));
				uxClass.Attributes.Add(propertyValues[i].Name, new UxString(propertyValues[i].DefaultValue));
			}
		}

		private void Apply(UxNode uxClass, List<PropertyValue> propertyValues)
		{
			foreach (var child in uxClass.Children.Where(c => c is UxNode).Cast<UxNode>())
			{
				Apply(child, propertyValues);
			}

			if (uxClass.ClassName == "Text")
			{
				var layerName = uxClass.SketchLayerName;
				switch (NameValidator.NameIsValid(layerName))
				{
					case NameValidity.InvalidCharacter:
                        _log.Warning($"Could not create a text property for the layer '{layerName}', as it contains an invalid character. Please only use the letters a-z, numbers, or underscores, and don't start the name with a number.");
						break;
					case NameValidity.InvalidKeyword:
                        _log.Warning($"Could not create a text property for the layer '{layerName}', as '{layerName}' is a reserved word. Please choose another name.");
						break;
					case NameValidity.Valid:
						if (propertyValues.Any(p => p.Name.Equals(layerName)))
						{
                            _log.Warning($"Could not create a text property for the layer '{layerName}', as a text property for another layer with the same name has already been created. Please use unique names for text layers within the same symbol.");
						}
						else
						{
							var defaultValue = (UxString) uxClass.Attributes["Value"];
							propertyValues.Add(new PropertyValue(layerName, defaultValue.Value));
							uxClass.Attributes["Value"] = new UxString("{Property " + layerName + "}");
						}
						break;
					default:
						throw new NotImplementedException("Internal error");
				}
			}
		}

		private static UxNode StringProperty(string name)
		{
			var property = new UxNode("string");
			property.Attributes["ux:Property"] = new UxString(name);
			return property;
		}

		private class PropertyValue
		{
			public readonly string Name;
			public readonly string DefaultValue;

			public PropertyValue(string name, string defaultValue)
			{
				Name = name;
				DefaultValue = defaultValue;
			}
		}
	}
}
