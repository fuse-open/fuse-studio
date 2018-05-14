namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody6 : Test
	{
		/*[Test]
		public void MethodBody600()
		{
			AssertCode(
@"using Uno;

class Interpreter
{
	public void Run(string[] input)
	{
		if (input.Length > 25) throw new Exception(""Input must be 25 or less characters tall"");
		foreach (var s in input)
		{
			if (s.Length > 80) throw new Exception(""Input must be 80 or less characters wide"");
		}

		string[] grid = new String[25];
		for (int i = 0; i < 25; i++)
		{
			grid[i] = """";
			for (int j = 0; j < 80; j++)
			{
				if (i < input.Length && j < input[i].Length)
				{
					grid[i] += input[i][j];
				}
				else
				{
					grid[i] += ' ';
				}
			}
		}

		var ip = int2(0);
		var ipDir = int2(1, 0);
		var stack = new Stack<int>(1000);
		var random = new Random(12345);
		bool done = false;
		while (!done)
		{
			var c = grid[ip.Y][ip.X];
			if (c >= '0' && c <= '9')
			{
				stack.Push((int)c - '0');
			}
			else
			{
				switch (c)
				{
					case '+':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a + b);
						}
						break;
						
					case '-':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b - a);
						}
						break;
						
					case '*':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a * b);
						}
						break;
						
					case '/':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b / a);
						}
						break;
						
					case '%':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b % a);
						}
						break;
						
					case '!': stack.Push(stack.Pop() == 0 ? 1 : 0); break;
						
					case '\'':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b > a ? 1 : 0);
						}
						break;
						
					case '>': ipDir = int2( 1,  0); break;
					case '<': ipDir = int2(-1,  0); break;
					case '^': ipDir = int2( 0, -1); break;
					case 'v': ipDir = int2( 0,  1); break;
						
					case '?':
						{
							switch (random.NextInt(4))
							{
								case 0:
								default:
									ipDir = int2( 1,  0);
									break;
								
								case 1: ipDir = int2(-1,  0); break;
								case 2: ipDir = int2( 0, -1); break;
								case 3: ipDir = int2( 0,  1); break;
							}
						}
						break;
						
					case '_':
						switch (stack.Pop())
						{
							case 0: ipDir = int2( 1,  0); break;
							default: ipDir = int2(-1,  0); break;
						}
						break;
						
					case '|':
						switch (stack.Pop())
						{
							case 0: ipDir = int2( 0,  1); break;
							default: ipDir = int2( 0, -1); break;
						}
						break;
						
					case '""':
						// TODO: string mode
						break;
						
					case ':':
						{
							var a = stack.Pop();
							stack.Push(a);
							stack.Push(a);
						}
						break;
						
					case '\\':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a);
							stack.Push(b);
						}
						break;
						
					case '$': stack.Pop(); break;
						
					case '.': Uno.Platform.System.Log(stack.Pop().ToString()); break;
					case ',': Uno.Platform.System.Log(((char)stack.Pop()).ToString()); break;
						
					case '#':
				}
			}
			ip = int2((ip.X + ipDir.X) % 80, (ip.Y + ipDir.Y) % 25);

			break;
		}
		
		static void step($(ref,out)
	}
}

"
			);
		}

		[Test]
		public void MethodBody601()
		{
			AssertCode(
@"

using Uno;

class Interpreter
{
	public void Run(string[] input)
	{
		if (input.Length > 25) throw new Exception(""Input must be 25 or less characters tall"");
		foreach (var s in input)
		{
			if (s.Length > 80) throw new Exception(""Input must be 80 or less characters wide"");
		}

		string[] grid = new String[25];
		for (int i = 0; i < 25; i++)
		{
			grid[i] = """";
			for (int j = 0; j < 80; j++)
			{
				if (i < input.Length && j < input[i].Length)
				{
					grid[i] += input[i][j];
				}
				else
				{
					grid[i] += ' ';
				}
			}
		}

		var ip = int2(0);
		var ipDir = int2(1, 0);
		var stack = new Stack<int>(1000);
		var random = new Random(12345);
		bool done = false;
		while (!done)
		{
			var c = grid[ip.Y][ip.X];
			if (c >= '0' && c <= '9')
			{
				stack.Push((int)c - '0');
			}
			else
			{
				switch (c)
				{
					case '+':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a + b);
						}
						break;
						
					case '-':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b - a);
						}
						break;
						
					case '*':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a * b);
						}
						break;
						
					case '/':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b / a);
						}
						break;
						
					case '%':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b % a);
						}
						break;
						
					case '!': stack.Push(stack.Pop() == 0 ? 1 : 0); break;
						
					case '\'':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(b > a ? 1 : 0);
						}
						break;
						
					case '>': ipDir = int2( 1,  0); break;
					case '<': ipDir = int2(-1,  0); break;
					case '^': ipDir = int2( 0, -1); break;
					case 'v': ipDir = int2( 0,  1); break;
						
					case '?':
						{
							switch (random.NextInt(4))
							{
								case 0:
								default:
									ipDir = int2( 1,  0);
									break;
								
								case 1: ipDir = int2(-1,  0); break;
								case 2: ipDir = int2( 0, -1); break;
								case 3: ipDir = int2( 0,  1); break;
							}
						}
						break;
						
					case '_':
						switch (stack.Pop())
						{
							case 0: ipDir = int2( 1,  0); break;
							default: ipDir = int2(-1,  0); break;
						}
						break;
						
					case '|':
						switch (stack.Pop())
						{
							case 0: ipDir = int2( 0,  1); break;
							default: ipDir = int2( 0, -1); break;
						}
						break;
						
					case '""':
						// TODO: string mode
						break;
						
					case ':':
						{
							var a = stack.Pop();
							stack.Push(a);
							stack.Push(a);
						}
						break;
						
					case '\\':
						{
							var a = stack.Pop();
							var b = stack.Pop();
							stack.Push(a);
							stack.Push(b);
						}
						break;
						
					case '$': stack.Pop(); break;
						
					case '.': Uno.Platform.System.Log(stack.Pop().ToString()); break;
					case ',': Uno.Platform.System.Log(((char)stack.Pop()).ToString()); break;
						
					case '#':
				}
			}
			step($(ref)

			break;
		}
		
		static void step(ref int2 ip, int2 ipDir)
		{
			ip = int2((ip.X + ipDir.X) % 80, (ip.Y + ipDir.Y) % 25);
		}
	}
}"
			);
		}*/

	}
}