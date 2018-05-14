namespace Outracks.Fuse.Templates
{
	public interface ITemplateVariableResolver
	{
		string ResolveVariable(string name);
	    bool HasVariable(string name);
	}
}