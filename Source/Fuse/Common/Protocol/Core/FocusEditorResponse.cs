namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("FocusEditor")]
	public class FocusEditorResponse : IResponseData
	{
		[PluginComment("Handle to focus, only used on Windows platform")]
		public int? FocusHwnd;
	}
}