using Uno;
using Fuse;

public partial class Application
{
	public Application()
	{
		new ForeignAccelerometer.AccelerometerModule();
		InitializeUX();
	}
}
