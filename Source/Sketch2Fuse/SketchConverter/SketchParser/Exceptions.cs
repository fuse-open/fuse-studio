using System;

namespace SketchConverter.SketchParser
{
	public class SketchParserException : Exception
	{
		public SketchParserException(string message, Exception innerException = null) : base(message, innerException)
		{
		}
	}

	public class InvalidAttributeException : Exception
	{
		public string Key { get; }
		public string ExpectedValue { get; }
		public string ActualValue { get; }

		public InvalidAttributeException(string key, string expectedValue, string actualValue)
			: base($"Expected value for '{key}' to equal '{expectedValue}', got '{actualValue}")
		{
			Key = key;
			ExpectedValue = expectedValue;
			ActualValue = actualValue;
		}
	}

}
