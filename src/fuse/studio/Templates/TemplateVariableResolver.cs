using System;

namespace Outracks.Fuse.Templates
{
	public class TemplateVariableResolver : ITemplateVariableResolver
	{
		readonly ITemplateVariableResolver _parentScope;
		readonly string _key;
		readonly string _value;

		public TemplateVariableResolver(ITemplateVariableResolver parentScope = null, string key = null, string value = null)
		{
			_parentScope = parentScope;
			_key = key;
			_value = value;
		}

	    public bool HasVariable(string name)
	    {
	        if (name == _key) return true;
            return _parentScope!=null && _parentScope.HasVariable(name);
	    }

		public string ResolveVariable(string name)
		{
			name.ThrowIfNull("name");

			if (name == _key)
				return _value;

			if (_parentScope != null)
				return _parentScope.ResolveVariable(name);

			throw new ArgumentException("Invalid environment variable", "name");
		}
	}

	public static class TemplateVariableResolverExtensions
	{
		public static ITemplateVariableResolver With(this ITemplateVariableResolver self, string key, string value)
		{
			return new TemplateVariableResolver(self, key, value);
		}
	}
}