using System.Diagnostics;
using System.Text;

namespace Outracks.Diagnostics.Reactive
{
	[DebuggerDisplay("{_rxMethodName,nq} called from {_callerFilePath,nq}:{_callerLineNumber,nq}")]
	public class ObservableTraceInfo
	{
		protected bool Equals(ObservableTraceInfo other)
		{
			return string.Equals(_rxMethodName, other._rxMethodName) && string.Equals(_callerFilePath, other._callerFilePath) &&
				_callerLineNumber == other._callerLineNumber;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ObservableTraceInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (_rxMethodName != null ? _rxMethodName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_callerFilePath != null ? _callerFilePath.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ _callerLineNumber;
				return hashCode;
			}
		}

		public static bool operator ==(ObservableTraceInfo left, ObservableTraceInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ObservableTraceInfo left, ObservableTraceInfo right)
		{
			return !Equals(left, right);
		}

		readonly string _rxMethodName;

		public string MethodName
		{
			get { return _rxMethodName; }
		}

		public string CallerFilePath
		{
			get { return _callerFilePath; }
		}

		public int CallerLineNumber
		{
			get { return _callerLineNumber; }
		}

		readonly string _callerFilePath;
		readonly int _callerLineNumber;

		public ObservableTraceInfo(string rxMethodName, string callerFilePath, int callerLineNumber)
		{
			_rxMethodName = rxMethodName;
			_callerFilePath = callerFilePath;
			_callerLineNumber = callerLineNumber;
		}

		public void AppendString(StringBuilder stringBuilder)
		{
			stringBuilder.AppendFormat("{0} called from {1}:{2}", _rxMethodName, _callerFilePath, _callerLineNumber);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			AppendString(sb);
			return sb.ToString();
		}
	}
}