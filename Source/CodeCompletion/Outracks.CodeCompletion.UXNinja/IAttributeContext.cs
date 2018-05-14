using Uno;

namespace Outracks.UnoDevelop.UXNinja
{
	public interface IAttributeContext
	{
		string Name { get; }

		string NamespacePrefix { get; }

		string FullName { get; }

		string Value { get; }

		Source Source { get; }

		bool IsNamespaceDeclaration { get; }

		T GetBackingStore<T>() where T : class;
	}
}
