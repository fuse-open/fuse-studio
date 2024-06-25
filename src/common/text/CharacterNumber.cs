using System;

namespace Outracks
{
	/// <summary>
	/// 1-indexed column/character index.
	/// One tab advances one Character.
	/// One multi-byte character advances one Character.
	/// The first character of a line will always have Character == 1, indifferent of CR.
	/// </summary>
	public struct CharacterNumber : IComparable<CharacterNumber>, IEquatable<CharacterNumber>
	{
		readonly int _characterMinusOne;

		public CharacterNumber(int character)
		{
			if (character <= 0) throw new ArgumentOutOfRangeException("character", "CharacterNumber must be >= 1");
			_characterMinusOne = character - 1;
		}

		public static implicit operator int(CharacterNumber line)
		{
			return line._characterMinusOne + 1;
		}

		public override string ToString()
		{
			return ((int) this).ToString();
		}

		public bool Equals(CharacterNumber other)
		{
			return _characterMinusOne == other._characterMinusOne;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is CharacterNumber && Equals((CharacterNumber)obj);
		}

		public override int GetHashCode()
		{
			return _characterMinusOne;
		}

		public static bool operator ==(CharacterNumber left, CharacterNumber right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CharacterNumber left, CharacterNumber right)
		{
			return !left.Equals(right);
		}

		public static bool operator <(CharacterNumber left, CharacterNumber right)
		{
			return left._characterMinusOne < right._characterMinusOne;
		}

		public static bool operator >(CharacterNumber left, CharacterNumber right)
		{
			return left._characterMinusOne > right._characterMinusOne;
		}

		public static bool operator <=(CharacterNumber left, CharacterNumber right)
		{
			return left._characterMinusOne <= right._characterMinusOne;
		}

		public static bool operator >=(CharacterNumber left, CharacterNumber right)
		{
			return left._characterMinusOne >= right._characterMinusOne;
		}

		public int CompareTo(CharacterNumber other)
		{
			return _characterMinusOne.CompareTo(other._characterMinusOne);
		}
	}
}