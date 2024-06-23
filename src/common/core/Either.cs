using System;

namespace Outracks
{
	public class Either<TL, TR>
	{
		readonly TL _left;
		readonly TR _right;

		readonly bool _isLeft;

		public Either(TL left)
		{
			_isLeft = true;
			_left = left;
		}

		public Either(TR right)
		{
			_isLeft = false;
			_right = right;
		}

		public T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight)
		{
			return _isLeft ? onLeft(_left) : onRight(_right);
		}

		public void Match(Action<TL> onLeft, Action<TR> onRight)
		{
			if (_isLeft)
				onLeft(_left);
			else
				onRight(_right);
		}
	}
}
