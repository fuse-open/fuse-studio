using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.EditorService.Tests
{
	class MessageDatabase
	{
		readonly string _name;
		readonly List<MessageDatabaseItem> _messageDatabaseItems;

		MessageDatabase(string name, List<MessageDatabaseItem> messageDatabaseItems)
		{
			_name = name;
			_messageDatabaseItems = messageDatabaseItems;
		}

		public static bool IsEqualWhileIgnoreComments(MessageDatabase a, MessageDatabase b, IObserver<string> errors)
		{
			if (a._name != b._name)
			{
				errors.OnNext(string.Format("Message name differs, A = {0}, B = {1}", a._name, b._name));
				return false;
			}

			if (a._messageDatabaseItems.Count != b._messageDatabaseItems.Count)
			{
				errors.OnNext(string.Format("Looks like you either have removed or added a new datafield"));
				return false;
			}

			foreach (var aItem in a._messageDatabaseItems)
			{
				var isEqual = b._messageDatabaseItems.FirstOrDefault(bItem => MessageDatabaseItem.IsEqualWhileIgnoreComments(aItem, bItem));

				if (isEqual == null)
				{
					var errorStringBuilder = new StringBuilder();
					errorStringBuilder.Append("A property/field has changed, expected to find: " + aItem.Name + "\nWidth: ");
					foreach (var child in aItem.Children)
						errorStringBuilder.Append(child.Name + ",");

					errors.OnNext(errorStringBuilder.ToString());
					return false;
				}
			}

			return true;
		}

		public void Dump(AbsoluteFilePath filePath)
		{
			Directory.CreateDirectory(filePath.ContainingDirectory.NativePath);

			using (var file = File.Open(filePath.NativePath, FileMode.Create))
			using (var writer = new BinaryWriter(file))
			{
				writer.Write(_name);
				writer.Write(_messageDatabaseItems.Count);
				foreach (var messageDatabaseItem in _messageDatabaseItems)
				{
					MessageDatabaseItem.Write(messageDatabaseItem, writer);
				}				
			}
		}

		public static MessageDatabase From(AbsoluteFilePath filePath)
		{			
			using(var file = File.Open(filePath.NativePath, FileMode.Open))
			using (var reader = new BinaryReader(file))
			{
				var databaseItems = new List<MessageDatabaseItem>();
				var name = reader.ReadString();
				var count = reader.ReadInt32();
				for (var i = 0; i < count; ++i)
				{
					databaseItems.Add(MessageDatabaseItem.Read(reader));
				}

				return new MessageDatabase(name, databaseItems);
			}			
		}

		public static MessageDatabase From(IMessagePayload message)
		{
			var databaseItems = new List<MessageDatabaseItem>();
			var properties = message.GetType().GetPropertiesAndFields();
			foreach (var propertyInfo in properties)
			{
				CreateDatabaseItem(propertyInfo).Do(databaseItems.Add);
			}

			return new MessageDatabase(message.GetPayloadType(), databaseItems);
		}

		static Optional<MessageDatabaseItem> CreateDatabaseItem(MemberInfo memberInfo)
		{
			var attrib = (PluginCommentAttribute) Attribute.GetCustomAttribute(memberInfo, typeof (PluginCommentAttribute));
			if (attrib == null)
				return Optional.None();

			var children = new List<MessageDatabaseItem>();
			var underlyingType = memberInfo.GetUnderlyingType();
			var genericArguments = underlyingType.GetGenericArguments();
			if (genericArguments.Length == 1)
			{
				underlyingType = genericArguments[0];
			}

			foreach (var propertyChild in underlyingType.GetPropertiesAndFields())
			{
				CreateDatabaseItem(propertyChild).Do(children.Add);
			}

			return new MessageDatabaseItem(memberInfo.Name, attrib, children);
		}
	}

	static class TypeHelper
	{
		public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type)
		{
			return type.GetProperties().OfType<MemberInfo>().Concat(type.GetFields());
		}

		public static Type GetUnderlyingType(this MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Event:
					return ((EventInfo)member).EventHandlerType;
				case MemberTypes.Field:
					return ((FieldInfo)member).FieldType;
				case MemberTypes.Method:
					return ((MethodInfo)member).ReturnType;
				case MemberTypes.Property:
					return ((PropertyInfo)member).PropertyType;
				default:
					throw new ArgumentException
					(
					 "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
					);
			}
		}
	}
}