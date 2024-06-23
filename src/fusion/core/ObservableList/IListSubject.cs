namespace Outracks.Fusion
{
	public interface IListSubject<T> : IListObserver<T>, IObservableList<T>
	{
	}
}