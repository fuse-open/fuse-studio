namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("FocusDesigner")]
	public class FocusDesignerRequest : IRequestData<FocusDesignerResponse>
	{
		[PluginComment("File path")]
		public string File;

		[PluginComment("Cursor line")]
		public int Line;

		[PluginComment("Cursor column")]
		public int Column;
	}
}