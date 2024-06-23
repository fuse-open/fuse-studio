namespace Outracks.Simulator.Bytecode
{
	static class ExpressionIdRegistry
	{
		public const char ReadVariable = 'a';
		public const char Literal = 'b';
		public const char Lambda = 'c';
		public const char MethodGroup = 'd';
		public const char IsType = 'e';
		public const char LogicalOr = 'f';
		public const char Instantiate = 'g';
		public const char CallLambda = 'h';
		public const char CallStaticMethod = 'i';
		public const char CallDynamicMethod = 'j';
		public const char ReadStaticField = 'k';
		public const char ReadProperty = 'l';
		public const char WriteProperty = 'm';
		public const char AddEventHandler = 'n';
		public const char RemoveEventHandler = 'o';
	}
}