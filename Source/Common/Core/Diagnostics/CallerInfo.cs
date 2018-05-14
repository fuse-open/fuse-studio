namespace Outracks
{
	public class CallerInfo
	{
		public readonly string Name;
		public readonly string SourceFilePath;
		public readonly int SourceLineNumber;

		public CallerInfo(string name, string sourceFilePath, int sourceLineNumber)
		{
			Name = name;
			SourceFilePath = sourceFilePath;
			SourceLineNumber = sourceLineNumber;
		}

		protected bool Equals(CallerInfo other)
		{
			return string.Equals(Name, other.Name) && SourceLineNumber == other.SourceLineNumber && string.Equals(SourceFilePath, other.SourceFilePath);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((CallerInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ SourceLineNumber;
				hashCode = (hashCode * 397) ^ (SourceFilePath != null ? SourceFilePath.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}