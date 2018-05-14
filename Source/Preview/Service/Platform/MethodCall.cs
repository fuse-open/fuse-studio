using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Fuse.Preview
{
	public class MethodCall
	{
		public string Method { get; private set; }

		public object[] Arguments { get; private set; }
		
		public static MethodCall FromExpression<T>(Expression<Action<T>> command)
		{
			var call = command.Body as MethodCallExpression;
			if (call == null)
				throw new NotSupportedException("Not method call: " + command.Body);

			var args = new object[call.Arguments.Count];
			for (int i =0; i<call.Arguments.Count; i++)
			{
				var arg = call.Arguments[i];
				var lambda = Expression.Lambda(arg, command.Parameters);
				var d = lambda.Compile();
				var value = d.DynamicInvoke(new object[1]);
				args[i] = value;
			}

			return new MethodCall
			{
				Arguments = args,
				Method = call.Method.Name,
			};
		}

		/// <exception cref="IOException"></exception>
		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(Method);
			writer.Write(Arguments.Length);
			foreach (var argument in Arguments)
				writer.WriteTaggedValue(argument);
		}
		
		/// <exception cref="IOException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static MethodCall ReadFrom(BinaryReader reader)
		{
			return new MethodCall
			{
				Method = reader.ReadString(),
				Arguments = Enumerable
					.Range(0, reader.ReadInt32())
					.Select(_ => reader.ReadTaggedValue())
					.ToArray(),
			};
		}
		
		public void InvokeOn(object self)
		{
			var method = self.GetType().GetMethod(Method);
			method.Invoke(self, Arguments);
		}

	}
}
