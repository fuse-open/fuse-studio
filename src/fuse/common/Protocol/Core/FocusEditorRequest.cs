namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("FocusEditor")]
	public class FocusEditorRequest : IRequestData<FocusEditorResponse>
	{
		[PluginComment("File path")]
		public string File;

		[PluginComment("Cursor line")]
		public int Line;

		[PluginComment("Cursor column")]
		public int Column;

		[PluginComment("Project file path")]
		public string Project;
	}
}