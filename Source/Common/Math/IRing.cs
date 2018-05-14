namespace Outracks
{
	public interface IRing<T> : IGroup<T>
	{
		T Mul(T other);

		T One { get; }

		T Reciprocal();
	}
}