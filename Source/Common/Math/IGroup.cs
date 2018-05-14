namespace Outracks
{
	public interface IGroup<T>
	{
		T Add(T other);
		T Inverse();

		T Zero { get; }
	}
}