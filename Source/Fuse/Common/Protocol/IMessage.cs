using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Outracks.Fuse.Protocol
{
	public interface IEventMessage<out T> : IMessage where T : IEventData
	{
		string Name { get; }
		T Data { get; }
		int SubscriptionId { get; }
	}

	public interface IRequestMessage<out T> : IMessage where T : IRequestData
	{
		int Id { get; }
		string Name { get; }
		T Arguments { get; }
	}

	public interface IResponseMessage<out T> : IMessage where T : IResponseData
	{
		int Id { get; }
		Status Status { get; }
		List<Error> Errors { get; }
		T Result { get; }
	}

/*	public interface IMessage<out T> : IMessage where T : IMessagePayload
	{		
		string Type { get; }
		T Data { get; }
	}*/

	public interface IMessagePayload
	{		
	}

	public interface IEventData : IMessagePayload
	{
	}
	
	public interface IRequestData<out TResponse> : IRequestData
	{
	}
	
	public interface IRequestData : IMessagePayload
	{		
	}

	public interface IResponseData : IMessagePayload
	{		
	}

	public interface IMessage
	{
		[JsonIgnore]
		string MessageType { get; }
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PayloadTypeNameAttribute : System.Attribute
	{
		public readonly string Name;
		public PayloadTypeNameAttribute(string name)
		{
			Name = name;
		}
	}

	public static class PayloadMessageExtensions
	{
		public static string GetPayloadType<T>(this T type) where T : IMessagePayload
		{
			var attrib = (PayloadTypeNameAttribute)System.Attribute.GetCustomAttribute(type.GetType(), typeof(PayloadTypeNameAttribute));
			if(attrib == null)
				throw new InvalidOperationException("Expected message data to have PayloadTypeNameAttribute.");

			return attrib.Name;
		}

		public static string GetPayloadTypeName(this Type type)
		{
			var attrib = (PayloadTypeNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(PayloadTypeNameAttribute));
			if (attrib == null)
				throw new InvalidOperationException("Expected message data to have PayloadTypeNameAttribute.");

			return attrib.Name;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
	public class PluginCommentAttribute : System.Attribute
	{
		public readonly string Comment;
		public readonly string Example;
		public readonly bool IsNumber;

		public PluginCommentAttribute(string comment, string example="")
		{
			Comment = comment;
			Example = example;
			IsNumber = false;
		}

		public PluginCommentAttribute(string comment, int example)
		{
			Comment = comment;
			Example = example.ToString(CultureInfo.InvariantCulture);
			IsNumber = true;
		}

		public PluginCommentAttribute(string comment, float example)
		{
			Comment = comment;
			Example = example.ToString(CultureInfo.InvariantCulture);
			IsNumber = true;
		}

		public PluginCommentAttribute(string comment, double example)
		{
			Comment = comment;
			Example = example.ToString(CultureInfo.InvariantCulture);
			IsNumber = true;
		}

		internal PluginCommentAttribute(string comment, string example, bool isNumber)
		{
			Comment = comment;
			Example = example.ToString(CultureInfo.InvariantCulture);
			IsNumber = isNumber;
		}
	}
}
