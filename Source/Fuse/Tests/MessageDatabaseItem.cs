using System.Collections.Generic;
using System.IO;
using System.Linq;
using Outracks.Fuse.Protocol;

namespace Outracks.EditorService.Tests
{
	class MessageDatabaseItem
	{
		public readonly string Name;
		public readonly PluginCommentAttribute Comment;
		public readonly List<MessageDatabaseItem> Children;

		public MessageDatabaseItem(string name, PluginCommentAttribute comment, List<MessageDatabaseItem> children)
		{
			Name = name;
			Comment = comment;
			Children = children;
		}

		public static bool IsEqualWhileIgnoreComments(MessageDatabaseItem a, MessageDatabaseItem b)
		{
			if (a.Name != b.Name)
				return false;

			if (a.Children.Count != b.Children.Count)
				return false;

			foreach (var childA in a.Children)
			{
				var isEqual = b.Children.FirstOrDefault(childB => IsEqualWhileIgnoreComments(childA, childB));
				if (isEqual == null)
					return false;
			}

			return true;
		}

		public static void Write(MessageDatabaseItem item, BinaryWriter writer)
		{
			writer.Write(item.Name);
			writer.Write(item.Comment.Comment);
			writer.Write(item.Comment.Example);
			writer.Write(item.Comment.IsNumber);
			writer.Write(item.Children.Count);

			foreach (var child in item.Children)
			{
				MessageDatabaseItem.Write(child, writer);
			}
		}

		public static MessageDatabaseItem Read(BinaryReader reader)
		{
			var name = reader.ReadString();
			var comment = reader.ReadString();
			var example = reader.ReadString();
			var isNumber = reader.ReadBoolean();
			var numChildren = reader.ReadInt32();

			var children = new List<MessageDatabaseItem>();
			for (var i = 0; i < numChildren; ++i)
			{
				children.Add(MessageDatabaseItem.Read(reader));
			}

			return new MessageDatabaseItem(name, new PluginCommentAttribute(comment, example, isNumber), children);
		}
	}
}