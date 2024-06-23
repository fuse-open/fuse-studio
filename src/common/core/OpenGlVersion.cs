namespace Outracks
{
	public class OpenGlVersion
	{
		public readonly string GlVersion;
		public readonly string GlVendor;
		public readonly string GlRenderer;

		public OpenGlVersion(string glVersion, string glVendor, string glRenderer)
		{
			GlVersion = glVersion;
			GlVendor = glVendor;
			GlRenderer = glRenderer;
		}
	}
}