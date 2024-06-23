using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks.Fuse.Protocol
{
	public interface IPayloadParserContext
	{
		Optional<Type> GetCommandType(string name, Type expectedDataInterface);
	}

	public class PayloadParserContext : IPayloadParserContext
	{
		readonly Dictionary<Type, string> _payloadTypes = new Dictionary<Type, string>();

		public PayloadParserContext(params Type []payloadTypes)
		{
			AddValidPayloadTypes(payloadTypes);
		}

		void AddValidPayloadTypes(IEnumerable<Type> payloadTypes)
		{
			foreach (var payloadType in payloadTypes)
			{
				_payloadTypes.Add(payloadType, payloadType.GetPayloadTypeName());
			}
		}

		public Optional<Type> GetCommandType(string name, Type expectedDataInterface)
		{
			foreach (var payloadType in _payloadTypes)
			{
				if(!payloadType.Key.GetInterfaces().Contains(expectedDataInterface))
					continue;

				if(EqualsTypeWithName(payloadType.Value, name))
					return payloadType.Key;
			}

			return Optional.None();
		}

		static bool EqualsTypeWithName(string typeName, string name)
		{
			return typeName == name;
		}
	}
}