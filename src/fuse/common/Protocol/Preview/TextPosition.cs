namespace Outracks.Fuse.Protocol.Messages
{
	public class TextPosition
	{
		[PluginComment("Line is expected to be 1 indexed.")]
		public int Line;

		[PluginComment("Character is expected to be 1 indexed.")]
		public int Character;
	}
}